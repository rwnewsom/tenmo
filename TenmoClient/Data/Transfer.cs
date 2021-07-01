using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// Creates a transfer object
    /// </summary>
    public class Transfer
    {
        /// <summary>
        /// The ID of the user SENDING funds
        /// </summary>
        public int FromUserId { get; set; }

        /// <summary>
        /// The ID of the user RECEIVING funds
        /// </summary>
        public int ToUserId { get; set; }

        /// <summary>
        /// The amount in US Dollars to be transferred
        /// </summary>
        public decimal Amount { get; set; }
    }
}
