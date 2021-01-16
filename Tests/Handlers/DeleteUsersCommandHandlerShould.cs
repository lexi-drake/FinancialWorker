using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Worker;

namespace Tests
{
    public class DeleteUsersCommandHandlerShould
    {
        private Mock<IUserRespository> _repo;

        private DeleteUsersCommandHandler _handler;

        public DeleteUsersCommandHandlerShould()
        {
            _repo = new Mock<IUserRespository>();

            _handler = new DeleteUsersCommandHandler(new Mock<ILogger<DeleteUsersCommandHandler>>().Object, _repo.Object);
        }

        [Fact]
        public async Task DeletesUsers()
        {
            var count = new Random().Next(1000);
            var userIds = new List<string>();
            for (var i = 0; i < count; i++)
            {
                userIds.Add(Guid.NewGuid().ToString());
            }

            var command = new DeleteUsersCommand()
            {
                UserIds = userIds
            };

            await _handler.Handle(command, new CancellationToken());

            _repo.Verify(x => x.DeleteUserByIdAsync(It.IsAny<string>()), Times.Exactly(count));
        }
    }
}