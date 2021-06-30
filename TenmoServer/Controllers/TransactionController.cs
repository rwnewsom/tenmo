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
        private UserSqlDAO userSqlDAO;



        public TransactionController(UserSqlDAO userSqlDAO)
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
                return Ok(account.Balance);
            }
            else
            {
                return NotFound("Account balance does not exist");
            } 

        }
        
    }
}
