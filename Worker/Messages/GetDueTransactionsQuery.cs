using System.Collections.Generic;
using MediatR;

namespace Worker
{
    public class GetDueTransactionsQuery : IRequest<IEnumerable<RecurringTransaction>> { }
}