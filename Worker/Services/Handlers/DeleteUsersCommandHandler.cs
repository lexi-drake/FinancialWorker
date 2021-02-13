using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace Worker
{
    public class DeleteUsersCommandHandler : IRequestHandler<DeleteUsersCommand>
    {
        private ILogger _logger;
        private IUserRespository _repo;

        public DeleteUsersCommandHandler(ILogger logger, IUserRespository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<Unit> Handle(DeleteUsersCommand command, CancellationToken cancellation)
        {
            foreach (var id in command.UserIds)
            {
                _logger.Information($"Deleting user {id}.");
                await _repo.DeleteUserByIdAsync(id);
                await _repo.DeleteMessagesByRecipientIdAsync(id);
            }
            return Unit.Value;
        }
    }
}