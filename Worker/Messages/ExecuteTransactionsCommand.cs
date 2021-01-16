using System.Collections.Generic;
using MediatR;

namespace Worker
{
    public class ExecuteTransactionsCommand : IRequest
    {
        public IEnumerable<RecurringTransaction> RecurringTransactions { get; set; }
    }
}