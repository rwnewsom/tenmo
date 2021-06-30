using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
/// <summary>
/// Return value from API endpoint //added class
/// </summary>
    public class Account
    {
        public int Account_Id { get; set; }
        public int User_Id { get; set; }
        public decimal Balance { get; set; }
    }
}
