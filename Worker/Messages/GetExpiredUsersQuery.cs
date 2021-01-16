using System.Collections.Generic;
using MediatR;

namespace Worker
{
    public class GetExpiredUsersQuery : IRequest<IEnumerable<string>> { }
}