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
        private int currentUserId = -1; //TODO: wipe these on logout!
        private string currentUserName = "";

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
                Console.WriteLine($"Welcome to TEnmo, {currentUserName}! Please make a selection: ");
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
                            GetUserTransfers();// TODONE!
                            break;
                        case 3: // View Pending Requests
                            Console.WriteLine("NOT IMPLEMENTED!"); // TODO: Implement me
                            break;
                        case 4: // Send TE Bucks
                            GetRecipientUsers(); // !TODONE
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
                    currentUserName = user.Username;
                }
            }
        }

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
            string toUserName = "";
            bool validRecipient = false;

            foreach (RecipientUser r in recipients)
            {
                if (recipientId == r.UserId)
                {
                    validRecipient = true;
                    toUserName = r.UserName;
                }
            }

            if (!validRecipient)
            {
                Console.WriteLine("Error, recipient not found.");
                return;
            }

            decimal transferAmount = GetTransferAmount();
            Account account = transactionService.Balance();
            decimal balance = account.Balance;

            if (transferAmount > balance)
            {
                Console.WriteLine("Amount requested exceeds funds available.");
            }
            else
            {
                Transfer transfer = transactionService.CreateTransfer(currentUserId, recipientId, transferAmount, currentUserName, toUserName);
                Console.WriteLine($"Success!FROM: {transfer.FromUserId}\nTO: {transfer.ToUserId}\nAMOUNT: {transfer.Amount}\nID: {transfer.TransferId} ");
            }
        }


        public void GetUserTransfers()
        {
            Console.Clear();
            List<Transfer> userTransfers = transactionService.GetUserTransfers();
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Transfers                                  ");
            Console.WriteLine("ID          From/To                  Amount");
            Console.WriteLine("-------------------------------------------");
            if(userTransfers.Count == 0)
            {
                Console.WriteLine($"\n\t*_*_*NO TRANSFERS FOUND*_*_*");
            }
            
            foreach (Transfer t in userTransfers)
            {
                Console.WriteLine($"{t.TransferId.ToString().PadRight(10)}From:{t.FromUserName.PadRight(15)}To:{t.ToUserName.PadRight(15)}{t.Amount.ToString("c")}");
            }

            int requested = consoleService.PromptForTransferID("review");
            bool isTransferValid = false;

            foreach (Transfer t in userTransfers)
            {
                if (t.TransferId == requested)
                {
                    isTransferValid = true;
                    Console.WriteLine("Id: " + t.TransferId);
                    Console.WriteLine("From: " + t.FromUserName);
                    Console.WriteLine("To: " + t.ToUserName);
                    Console.WriteLine("Type: " + t.TypeDescription);
                    Console.WriteLine("Status: " + t.StatusDescription);
                    Console.WriteLine("Amount: " + t.Amount.ToString("c"));
                }
            }
            if (!isTransferValid)
            {
                Console.WriteLine("Error: requested transfer not found.");
            }
        }

        public decimal GetTransferAmount()
        {
            decimal transferAmount = consoleService.PromptForDecimal("How Much would you like to transfer?");
            Console.WriteLine("Amount requested: " + transferAmount.ToString("c"));
            return transferAmount;
        }

    }
}
