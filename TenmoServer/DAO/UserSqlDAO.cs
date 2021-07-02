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
        public Transfer PostTransfer(Transfer transfer)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // INSERT INTO transfers VALUES(1001, 2001, @accountFrom, @accountTo); //TODO: need to get account numbers!
                SqlCommand cmd = new SqlCommand("UPDATE accounts SET balance = balance - @amount WHERE user_id = @fromUser_id; UPDATE accounts SET balance = balance + @amount WHERE user_id = @toUser_id; SELECT @@IDENTITY;", conn);
                cmd.Parameters.AddWithValue("@amount", transfer.Amount);
                cmd.Parameters.AddWithValue("@toUser_id", transfer.ToUserId);
                cmd.Parameters.AddWithValue("@fromUser_id", transfer.FromUserId);
                //transfer.TransferId = Convert.ToInt32(cmd.ExecuteScalar());       //keeps returning null...    because we aren't inserting anything!   
                //int createdId = Convert.ToInt32(cmd.ExecuteScalar());
                //transfer.TransferId = createdId;
                cmd.ExecuteNonQuery();
            }
            return transfer;
        }
    }
}

