using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class UserSqlDAO : IUserDAO
    {
        private readonly string connectionString;
        const decimal startingBalance = 1000;

        const string sqlGetTransfers = "SELECT  t.transfer_id AS 'transferId', (SELECT u.username WHERE t.account_from = a.account_id)  AS 'fromName', " +
            "(SELECT username FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id)  AS 'toName', t.amount AS 'amount', " +
            "tt.transfer_type_desc AS 'typeDescription', ts.transfer_status_desc AS 'statusDescription'  FROM transfers t " +
            "INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts  " +
            "ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id WHERE account_from = @accountId OR account_to = @accountId";

        public UserSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public User GetUser(string username)
        {
            User returnUser = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users WHERE username = @username", conn);
                cmd.Parameters.AddWithValue("@username", username);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows && reader.Read())
                {
                    returnUser = GetUserFromReader(reader);
                }
            }

            return returnUser;
        }

        public List<User> GetUsers()
        {
            List<User> returnUsers = new List<User>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        User u = GetUserFromReader(reader);
                        returnUsers.Add(u);
                    }

                }
            }

            return returnUsers;
        }

        public User AddUser(string username, string password)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO users (username, password_hash, salt) VALUES (@username, @password_hash, @salt)", conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                cmd.Parameters.AddWithValue("@salt", hash.Salt);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("SELECT @@IDENTITY", conn);
                int userId = Convert.ToInt32(cmd.ExecuteScalar());

                cmd = new SqlCommand("INSERT INTO accounts (user_id, balance) VALUES (@userid, @startBalance)", conn);
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@startBalance", startingBalance);
                cmd.ExecuteNonQuery();
            }

            return GetUser(username);
        }

        private User GetUserFromReader(SqlDataReader reader)
        {
            return new User()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
            };
        }

        public Account GetUserBalanceFromReader(int userId)
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT balance, account_id AS 'aid', a.user_id AS 'userid' FROM accounts a INNER JOIN users u on a.user_id = u.user_id WHERE u.user_id = @userid", conn);
                cmd.Parameters.AddWithValue("@userid", userId);
                SqlDataReader reader = cmd.ExecuteReader();

                Account account = new Account();
                while (reader.HasRows && reader.Read()) //changed from if
                {

                    account.UserId = Convert.ToInt32(reader["userid"]);
                    account.AccountId = Convert.ToInt32(reader["aid"]);
                    account.Balance = Convert.ToDecimal(reader["balance"]);
                    return account;
                }
                return null;
            }
        }
        private RecipientUser GetRecipientUserFromReader(SqlDataReader reader)
        {
            return new RecipientUser()
            {
                UserId = Convert.ToInt32(reader["userid"]),
                Username = Convert.ToString(reader["username"])
            };
        }
        //ToDo: add server piece
        public List<RecipientUser> GetRecipientUsers()
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT user_id AS 'userid', username FROM users", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                List<RecipientUser> returnUsers = new List<RecipientUser>();//changed position

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        RecipientUser u = GetRecipientUserFromReader(reader);
                        returnUsers.Add(u);
                    }

                }
                return returnUsers;
            }

        }
        /*
         *  "SELECT  t.transfer_id AS 'transferId', (SELECT u.username WHERE t.account_from = a.account_id)  AS 'fromName', " +
            "(SELECT username FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id)  AS 'toName', t.amount AS 'amount', " +
            "tt.transfer_type_desc AS 'typeDescription', ts.transfer_status_desc AS 'statusDescription'  FROM transfers t " +
            "INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts  " +
            "ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id WHERE account_from = @accountId OR account_to = @accountId";
         */


        private Transfer GetTransferFromReader(SqlDataReader reader)
        {
            return new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transferId"]),
                FromUserName = Convert.ToString(reader["fromName"]),
                ToUserName = Convert.ToString(reader["toName"]),
                TypeDescription = Convert.ToString(reader["typeDescription"]),
                StatusDescription = Convert.ToString(reader["statusDescription"]),
                Amount = Convert.ToInt32(reader["amount"])
            };
        }
        

        
        public List<Transfer> GetUserTransfers(int accountId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(sqlGetTransfers, conn);
                cmd.Parameters.AddWithValue("@accountId", accountId);
                SqlDataReader reader = cmd.ExecuteReader();
                
                List<Transfer> transfers = new List<Transfer>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Transfer t = GetTransferFromReader(reader);
                        transfers.Add(t);
                    }
                }
                return transfers;
            }


        }
        

        public Transfer PostTransfer(Transfer transfer)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE accounts SET balance = balance - @withdrawamount WHERE user_id = @fromUser_id; UPDATE accounts SET balance = balance + @amount WHERE user_id = @toUser_id; INSERT INTO transfers VALUES(1001, 2001, @accountFrom, @accountTo, @amount); SELECT @@IDENTITY;", conn);
                cmd.Parameters.AddWithValue("@withdrawamount", transfer.Amount);
                cmd.Parameters.AddWithValue("@amount", transfer.Amount);
                cmd.Parameters.AddWithValue("@toUser_id", transfer.ToUserId);
                cmd.Parameters.AddWithValue("@fromUser_id", transfer.FromUserId); //
                cmd.Parameters.AddWithValue("@accountFrom", transfer.AccountFrom);
                cmd.Parameters.AddWithValue("@accountTo", transfer.AccountTo);
                int createdId = Convert.ToInt32(cmd.ExecuteScalar());
                transfer.TransferId = createdId;
            }
            return transfer;
        }
    }
}

