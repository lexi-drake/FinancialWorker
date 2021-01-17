using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Worker;
using Serilog;

namespace Tests
{
    public class GetExpiredUsersQueryHandlerShould
    {
        private Mock<IUserRespository> _repo;
        private GetExpiredUsersQueryHandler _handler;

        public GetExpiredUsersQueryHandlerShould()
        {
            _repo = new Mock<IUserRespository>();
            _repo.Setup(x => x.GetUsersByLastLoggedInAsync(It.IsAny<DateTime>()))
                .Returns(Task.FromResult(GetListOfUsers()));

            _handler = new GetExpiredUsersQueryHandler(new Mock<ILogger>().Object, _repo.Object);
        }

        [Fact]
        public async Task ReturnsUserIds()
        {
            var query = new GetExpiredUsersQuery();

            var userIds = await _handler.Handle(query, new CancellationToken());
            Assert.NotEmpty(userIds);

            _repo.Verify(x => x.GetUsersByLastLoggedInAsync(It.IsAny<DateTime>()), Times.Once);
        }

        private IEnumerable<User> GetListOfUsers()
        {
            var count = new Random().Next(1, 1000);
            var transactions = new List<User>();
            for (var i = 0; i < count; i++)
            {
                transactions.Add(new User()
                {
                    Id = Guid.NewGuid().ToString()
                });
            }
            return transactions;
        }
    }
}