using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.DAO;
using TenmoServer.Models;
using TenmoServer.Security;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private IUserDAO userSqlDAO;



        public TransactionController(IUserDAO userSqlDAO)
        {
            this.userSqlDAO = userSqlDAO;
            
        }

        
        [HttpGet]
        public IActionResult GetBalance()
        {
            int userId = int.Parse(this.User.FindFirst("sub").Value);
            Account account = userSqlDAO.GetUserBalanceFromReader(userId);
            if(account.Balance >= 0)
            {
                return Ok(account);
            }
            else
            {
                return NotFound("Account balance does not exist");
            } 

        }
        
        [HttpGet("recipients")]
        public ActionResult<List<RecipientUser>> GetRecipientUsers()
        {
             return Ok(userSqlDAO.GetRecipientUsers());
        }

        [HttpGet("transfer")]
        public ActionResult<List<Transfer>> GetUserTransfers()
        {
            int userId = int.Parse(this.User.FindFirst("sub").Value);
            Account userAccount = userSqlDAO.GetUserBalanceFromReader(userId);
            int userAccountId = userAccount.AccountId;

            return Ok(userSqlDAO.GetUserTransfers(userAccountId));
        }

        [HttpPost("transfer")]
        public ActionResult SendMoney(Transfer transfer)
        {
            Account sendingAccount = userSqlDAO.GetUserBalanceFromReader(transfer.FromUserId);
            Account receivingAccount = userSqlDAO.GetUserBalanceFromReader(transfer.ToUserId);
            if (sendingAccount.Balance < transfer.Amount)
            {
                throw new Exception("Overdraft not permitted");
            }
            transfer.AccountFrom = receivingAccount.AccountId;
            transfer.AccountTo = sendingAccount.AccountId;
            Transfer newTransfer = this.userSqlDAO.PostTransfer(transfer);
            return Created($"/transfer/{newTransfer.TransferId}", newTransfer);

        }


        
    }
}
