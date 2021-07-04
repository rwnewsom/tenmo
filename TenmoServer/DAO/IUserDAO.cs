using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IUserDAO
    {
        User GetUser(string username);
        User AddUser(string username, string password);
        List<User> GetUsers();
        Account GetUserBalanceFromReader(int userId);
        List<RecipientUser> GetRecipientUsers();
        List<Transfer> GetUserTransfers(int userId);
        Transfer PostTransfer(Transfer transfer);
    }

}
