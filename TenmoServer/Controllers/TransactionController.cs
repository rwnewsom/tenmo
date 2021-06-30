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
        private readonly IUserDAO userSqlDAO;


        public TransactionController(IUserDAO _userSqlDAO)
        {
            userSqlDAO = _userSqlDAO;
            
        }

        /*
        [HttpGet]
        public IActionResult GetBalance(User user)
        {
            decimal balance = userSqlDAO.GetUserBalanceFromReader(user.UserId);
            if(balance >= 0)
            {
                return Ok(balance);
            }
            else
            {
                return NotFound("Account balance does not exist");
            } 
        }
        */
    }
}
