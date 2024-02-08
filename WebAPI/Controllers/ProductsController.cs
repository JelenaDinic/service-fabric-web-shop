using Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        [Route("api/products")]
        public async Task<ActionResult> GetAllProducts()
        {
            var statelessProxy = ServiceProxy.Create<IProductInterface>(
                new Uri("fabric:/NewServiceFabricApplication/ProductsService"));

            var products = await statelessProxy.GetAllProducts();

            return Ok(products);
        }
    }

    

    
}
