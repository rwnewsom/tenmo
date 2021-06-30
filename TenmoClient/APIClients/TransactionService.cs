using System;
using RestSharp;
using RestSharp.Authenticators;
using TenmoClient.Data;

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
            RestRequest request = new RestRequest(BASE_URL); //added path
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
    }
}
