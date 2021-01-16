using System.Collections.Generic;
using MediatR;

namespace Worker
{
    public class DeleteUsersCommand : IRequest
    {
        public IEnumerable<string> UserIds { get; set; }
    }
}