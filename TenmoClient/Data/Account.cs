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
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
    }
}
