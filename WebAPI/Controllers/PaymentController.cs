using Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebAPI.Controllers
{
    public class PaymentController : ControllerBase
    {
        [HttpGet]
        [Route("api/pay/{customerId}")]
        public async Task<string> Pay(int customerId)
        {
            var statelessProxy = ServiceProxy.Create<IPaymentInterface>(
                new Uri("fabric:/NewServiceFabricApplication/PaymentService"));

            return await statelessProxy.ProcessPayment(customerId);
        }
    }
}
