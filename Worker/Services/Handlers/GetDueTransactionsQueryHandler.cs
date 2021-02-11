using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace Worker
{
    public class GetDueTransactionsQueryHandler : IRequestHandler<GetDueTransactionsQuery, IEnumerable<RecurringTransaction>>
    {
        private const int MAXIMUM_USER_AGE_DAYS = 45;
        private ILogger _logger;
        private ILedgerRepository _repo;

        public GetDueTransactionsQueryHandler(ILogger logger, ILedgerRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<IEnumerable<RecurringTransaction>> Handle(GetDueTransactionsQuery query, CancellationToken cancellation)
        {
            var frequencies = await _repo.GetFrequenciesAsync();

            var response = new List<RecurringTransaction>();
            foreach (var frequency in frequencies)
            {
                var lookBackDays = GetLookbackDays(frequency);
                var transactions = await _repo.GetRecurringTransactionsByFrequencyAndLastExecutedAsync(frequency.Id, DateTime.Now.AddDays(-lookBackDays));
                response.AddRange(transactions);
                foreach (var transaction in transactions)
                {
                    await _repo.UpdateRecurringTransactionLastTriggeredAsync(transaction.Id, DateTime.Now);
                }
                _logger.Information($"{frequency.Description}: {transactions.Count()}");
            }
            return response;
        }

        private int GetLookbackDays(Frequency frequency)
        {
            var days = frequency.ApproxTimesPerYear switch
            {
                52 => 7,                                                        // Weekly
                26 => 14,                                                       // Biweekly
                24 => GetSemimonthlyLookbackDays(),                             // Semimonthly                                                    
                12 => (DateTime.Now - DateTime.Now.AddMonths(-1)).TotalDays,    // Monthly
                4 => (DateTime.Now - DateTime.Now.AddMonths(-3)).TotalDays,     // Quarterly
                2 => (DateTime.Now - DateTime.Now.AddMonths(-6)).TotalDays,     // Semiannually
                1 => (DateTime.Now - DateTime.Now.AddYears(-1)).TotalDays,      // Annually
                _ => throw new ArgumentException($"Unknown frequency {frequency.Description}")
            };
            return (int)Math.Round(days);
        }

        private double GetSemimonthlyLookbackDays()
        {
            var today = DateTime.Now.Day;
            var daysInMonth = DateTime.Now.AddMonths(1).AddDays(-today).Day;

            // If today is in the first half of the month, use half of the number of days in
            // the previous month, otherwise use half of the number of days in the current month.
            // Thus, if today is March 3, look back 14 days (number of days in February, usually)
            // to February 17; however, if today is March 25, look back 15.5 (16) days to
            // March 9.
            if(today < daysInMonth / 2) 
            {
                return (DateTime.Now - DateTime.Now.AddMonths(-1)).TotalDays / 2;
            }
            else 
            {
                return (DateTime.Now.AddMonths(1) - DateTime.Now).TotalDays / 2;
            }
        }
    }
}