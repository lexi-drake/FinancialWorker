using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace Worker
{
    public class GetExpiredUsersQueryHandler : IRequestHandler<GetExpiredUsersQuery, IEnumerable<string>>
    {
        private const int MAXIMUM_USER_AGE_DAYS = 45;
        private ILogger _logger;
        private IUserRespository _repo;

        public GetExpiredUsersQueryHandler(ILogger logger, IUserRespository repo)
        {
            _logger = logger;
            _repo = repo;
        }
        public async Task<IEnumerable<string>> Handle(GetExpiredUsersQuery query, CancellationToken cancellation)
        {
            var expiration = DateTime.Now.AddDays(-MAXIMUM_USER_AGE_DAYS);
            return from user in await _repo.GetUsersByLastLoggedInAsync(expiration)
                   select user.Id;
        }
    }
}