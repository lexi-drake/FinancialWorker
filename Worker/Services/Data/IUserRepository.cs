using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worker
{
    public interface IUserRespository
    {
        Task<IEnumerable<User>> GetUsersByLastLoggedInAsync(DateTime since);
        Task DeleteUserByIdAsync(string id);
        Task DeleteMessagesByRecipientIdAsync(string id);
    }
}