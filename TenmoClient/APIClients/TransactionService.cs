using System;
using RestSharp;
using RestSharp.Authenticators;
using TenmoClient.Data;
using System.Net;
using System.Collections.Generic;

namespace TenmoClient//adding class to limit authServices responsibility
{
    public class TransactionService
    {
        private readonly string BASE_URL = "https://localhost:44315/";
        private readonly IRestClient client = new RestClient();
        //private readonly RestClient client = new RestClient();
        //method to set token, take in string token

        //or userservice get token within balance
        public TransactionService() //constructor stolen from sallyclient
        {
            this.BASE_URL = BASE_URL + "transaction";
            this.client = new RestClient();
        }


        public Account Balance() //change from derived property to method???  -- DONE
        {
            RestRequest request = new RestRequest(BASE_URL);
            request.AddHeader("Authorization", "bearer " + UserService.Token);

            IRestResponse<Account> response = client.Get<Account>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("Could not connect to the server");
                return null;
            }

            if (!response.IsSuccessful)
            {
                Console.WriteLine("Oh noes! An error occurred: " + response.StatusDescription);
                return null;
            }

            return response.Data;
        }

        public List<RecipientUser> GetRecipientUsers()
        {
            RestRequest request = new RestRequest(BASE_URL + "/recipients");
            request.AddHeader("Authorization", "bearer " + UserService.Token);//Added header

            IRestResponse<List<RecipientUser>> response = client.Get<List<RecipientUser>>(request);

            if (response.IsSuccessful && response.ResponseStatus == ResponseStatus.Completed)
            {
                return response.Data;
            }
            else
            {
                Console.WriteLine("An error occurred fetching users");

                return new List<RecipientUser>();
            }
        }

        public List<Transfer> GetUserTransfers()
        {
            RestRequest request = new RestRequest(BASE_URL + "/transfer");
            request.AddHeader("Authorization", "bearer " + UserService.Token);

            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);

            if (response.IsSuccessful && response.ResponseStatus == ResponseStatus.Completed)
            {
                return response.Data;
            }
            else
            {
                Console.WriteLine("An error occurred fetching users");
                return new List<Transfer>();
            }
        }



        public Transfer CreateTransfer(int currentUserId, int recipientId, decimal transferAmount, string fromUserName, string toUserName)
        {
            Transfer transfer = new Transfer()
            {
                ToUserId = recipientId,
                FromUserId = currentUserId,
                Amount = transferAmount,
                FromUserName = fromUserName,
                ToUserName = toUserName
            };

            RestRequest request = new RestRequest(BASE_URL + "/transfer");
            request.AddHeader("Authorization", "bearer " + UserService.Token);
            request.AddJsonBody(transfer);

            IRestResponse<Transfer> response = client.Post<Transfer>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("Could not connect to the server");
                return null;
            }

            if (!response.IsSuccessful)
            {
                Console.WriteLine("Oh noes! An error occurred: " + response.StatusDescription);
                return null;
            }

            Transfer createdTransfer = response.Data;

            return createdTransfer;

        }


    }
}
