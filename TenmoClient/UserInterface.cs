using System;
using System.Collections.Generic;
using TenmoClient.Data;

namespace TenmoClient
{
    public class UserInterface
    {
        private readonly ConsoleService consoleService = new ConsoleService();
        private readonly AuthService authService = new AuthService();
        private readonly TransactionService transactionService = new TransactionService();

        private bool quitRequested = false;
        private int currentUserId = -1;

        public void Start()
        {
            while (!quitRequested)
            {
                while (!authService.IsLoggedIn)
                {
                    ShowLogInMenu();
                }

                // If we got here, then the user is logged in. Go ahead and show the main menu
                ShowMainMenu();
            }
        }

        private void ShowLogInMenu()
        {
            Console.WriteLine("Welcome to TEnmo!");
            Console.WriteLine("1: Login");
            Console.WriteLine("2: Register");
            Console.Write("Please choose an option: ");

            if (!int.TryParse(Console.ReadLine(), out int loginRegister))
            {
                Console.WriteLine("Invalid input. Please enter only a number.");
            }
            else if (loginRegister == 1)
            {
                HandleUserLogin();
            }
            else if (loginRegister == 2)
            {
                HandleUserRegister();
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        private void ShowMainMenu()
        {
            int menuSelection;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else
                {
                    switch (menuSelection)
                    {
                        case 1: // View Balance
                            ShowAccountBalance(); // TODONE!
                            break;
                        case 2: // View Past Transfers
                            Console.WriteLine("NOT IMPLEMENTED!");
                             // TODO: Implement me
                            break;
                        case 3: // View Pending Requests
                            Console.WriteLine("NOT IMPLEMENTED!"); // TODO: Implement me
                            break;
                        case 4: // Send TE Bucks
                            GetRecipientUsers();
                            //Console.WriteLine("NOT IMPLEMENTED!"); // TODO: Implement me
                            break;
                        case 5: // Request TE Bucks
                            Console.WriteLine("NOT IMPLEMENTED!"); // TODO: Implement me
                            break;
                        case 6: // Log in as someone else
                            Console.WriteLine();
                            UserService.ClearLoggedInUser(); //wipe out previous login info
                            return; // Leaves the menu and should return as someone else
                        case 0: // Quit
                            Console.WriteLine("Goodbye!");
                            quitRequested = true;
                            return;
                        default:
                            Console.WriteLine("That doesn't seem like a valid choice.");
                            break;
                    }
                }
            } while (menuSelection != 0);
        }

        private void HandleUserRegister()
        {
            bool isRegistered = false;

            while (!isRegistered) //will keep looping until user is registered
            {
                LoginUser registerUser = consoleService.PromptForLogin();
                isRegistered = authService.Register(registerUser);
            }

            Console.WriteLine("");
            Console.WriteLine("Registration successful. You can now log in.");
        }

        private void HandleUserLogin()
        {
            while (!UserService.IsLoggedIn) //will keep looping until user is logged in
            {
                LoginUser loginUser = consoleService.PromptForLogin();
                API_User user = authService.Login(loginUser);
                if (user != null)
                {
                    UserService.SetLogin(user);
                    currentUserId = user.UserId;
                }
            }
        }

        
        //modified this
        private void ShowAccountBalance()
        {
            Console.Clear();
            Account account = transactionService.Balance();
            int userId = account.UserId;
            int accountId = account.AccountId;
            decimal balance = account.Balance;
            Console.WriteLine("User Id: " + userId); //added to test
            Console.WriteLine("Account Id: " + accountId); //added to test
            Console.WriteLine("Current balance is: " + balance.ToString("c"));
            //Console.WriteLine((decimal)authService.Balance);
        }
        
        public void GetRecipientUsers()
        {
            Console.Clear(); 
            Console.WriteLine("The Following Recipients are available:");
            List<RecipientUser> recipients = transactionService.GetRecipientUsers();
                Console.WriteLine("Id:".PadRight(10) + "Name".PadRight(20));
            foreach (RecipientUser r in recipients)
            {
                Console.WriteLine(r.UserId.ToString().PadRight(10) + r.UserName.PadRight(20));
            }
            Console.WriteLine();
            int recipientId = consoleService.PromptForRecipientID(); // for transfer object

            bool validRecipient = false;
            foreach (RecipientUser r in recipients) 
            {
                if (recipientId == r.UserId)
                {
                    validRecipient = true;
                }
            }

            if (!validRecipient)
            {
                Console.WriteLine("Error, recipient not found.");
                return;
            }            

            Console.WriteLine("You have selected user ID: " + recipientId);
            Console.WriteLine("You current ID: " + currentUserId); //for transfer object
            decimal transferAmount = GetTransferAmount();

            Account account = transactionService.Balance();
            //int userId = account.UserId;
            //int accountId = account.AccountId;
            decimal balance = account.Balance;

            if (transferAmount > balance)
            {
                Console.WriteLine("Amount requested exceeds funds available.");
            }
            else
            {
                Transfer transfer = transactionService.CreateTransfer(currentUserId,recipientId,transferAmount);
                Console.WriteLine($"FROM: {transfer.FromUserId}\nTO: {transfer.ToUserId}\nAMOUNT: {transfer.Amount}");
            }
            //manual test that object created
        }

        public decimal GetTransferAmount()
        {
            
            decimal transferAmount = consoleService.PromptForDecimal("How Much would you like to transfer?");
            Console.WriteLine("Amount requested: " + transferAmount.ToString("c"));
            return transferAmount;
        }

    }
}
