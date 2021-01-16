using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Worker
{
    public class DeleteUsersCommandHandler : IRequestHandler<DeleteUsersCommand>
    {
        private ILogger<DeleteUsersCommandHandler> _logger;
        private IUserRespository _repo;

        public DeleteUsersCommandHandler(ILogger<DeleteUsersCommandHandler> logger, IUserRespository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<Unit> Handle(DeleteUsersCommand command, CancellationToken cancellation)
        {
            foreach (var id in command.UserIds)
            {
                _logger.LogInformation($"Deleting user {id}.");
                await _repo.DeleteUserByIdAsync(id);
            }
            return Unit.Value;
        }
    }
}