using Communication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        [HttpPost]
        [Route("api/register")]
        public async Task Register([FromBody] Customer customer)
        {
            var partitionId = customer.Id % 3;
            var statefulProxy = ServiceProxy.Create<ICustomerInterface>(
                new Uri("fabric:/NewServiceFabricApplication/CustomersService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            await statefulProxy.AddCustomer(customer);

        }

        [HttpPost]
        [Route("api/login")]
        public async Task<int> Login([FromBody] SignInModel signInModel)
        {
            var partitionId = CalculatePartitionId(signInModel.Email, 3); // Assume you have a function to calculate partitionId based on email
            var statefulProxy = ServiceProxy.Create<ICustomerInterface>(
                new Uri("fabric:/NewServiceFabricApplication/CustomersService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            // Call the Login method on the service
            int loginResult = await statefulProxy.Login(signInModel.Email, signInModel.Password);

            return loginResult;
        }

        [HttpPost]
        [Route("api/update-customer")]
        public async Task UpdateCustomer([FromBody] Customer customer)
        {
            var partitionId = customer.Id % 3;
            var statefulProxy = ServiceProxy.Create<ICustomerInterface>(
                new Uri("fabric:/NewServiceFabricApplication/CustomersService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            await statefulProxy.UpdateCustomer(customer);

        }

        [HttpGet]
        [Route("api/get-by-id")]
        public async Task<Customer> GetCustomerById([FromQuery] int customerId)
        {
            var partitionId = customerId % 3;
            var statefulProxy = ServiceProxy.Create<ICustomerInterface>(
                new Uri("fabric:/NewServiceFabricApplication/CustomersService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            return await statefulProxy.GetCustomerById(customerId);
        }

        private int CalculatePartitionId(string email, int numberOfPartitions)
        {
            // Use a hash function to get an integer value from the email
            int hash = email.GetHashCode();

            // Take the modulo of the hash to determine the partition
            int partitionId = Math.Abs(hash) % numberOfPartitions;

            return partitionId;
        }
    }
}
