using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.Azure.ServiceBus;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SoCreate.ServiceFabric.PubSub;

namespace PaymentService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class PaymentService : StatelessService, IPaymentInterface
    {
        public PaymentService(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<string> ProcessPayment(int customerId)
        {
            try
            {
                List<Cart> carts = await GetCartsByCustomerService(customerId);

                foreach (var cart in carts)
                {
                    Product product = await GetProductByIdService(cart.ProductId);
                    if (product != null)
                    {
                        Customer customer = await GetCustomerByIdService(customerId);
                        if (customer != null)
                        {
                            if (cart.Amount * product.Price <= customer.Wallet)
                            {
                                if (cart.Amount <= product.Amount)
                                {
                                    await UpdateCustomerService(customer, cart.Amount * product.Price);
                                    //update prodyct amount
                                    await UpdateCartService(cart, cart.Amount);
                                    var brokerClient = new BrokerClient();
                                    brokerClient.PublishMessageAsync(new PublishedMessageOne { Content = "Order successfully placed!!" });



                                    return
                                        $"Processed payment for CustomerId: {customerId}, ProductId: {product.Id}, Amount: {cart.Amount}, TotalCost: {cart.Amount * product.Price}";
                                }
                                else
                                {
                                    return 
                                        $"Insufficient product amount for CustomerId: {customerId}, ProductId: {product.Id}, Cart Amount: {cart.Amount}, Product Amount: {product.Amount}";
                                }
                            }
                            else
                            {
                                return
                                    $"Insufficient funds for CustomerId: {customerId}, ProductId: {product.Id}, Cart Amount: {cart.Amount}, TotalCost: {cart.Amount * product.Price}";
                            }
                        }
                        else
                        {
                            return
                                $"Customer not found for CustomerId: {customerId}";
                        }
                    }
                    else
                    {
                        return
                            $"Product not found for CustomerId: {customerId}, ProductId: {cart.ProductId}";
                    }
                }

                return "Nothing in cart.";
            }
            catch (Exception e)
            {
                return $"Error processing payment: {e.Message}";
            }
        }

        private async Task<List<Cart>> GetCartsByCustomerService(int customerId)
        {
            // Invoke another service to get carts by customer
            try
            {
                var partitionId = customerId % 3;
                var statefulProxy = ServiceProxy.Create<ICartInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                var carts = await statefulProxy.GetCartsByCustomer(customerId);

                return carts;
            }
            catch (Exception ex)
            {
                // Handle exception
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error getting carts by customer: {ex.Message}");
                return new List<Cart>();
            }
        }

        private async Task UpdateCartService(Cart cart, int amount)
        {
            // Invoke another service to get carts by customer
            try
            {
                var partitionId = cart.Id % 3;
                var statefulProxy = ServiceProxy.Create<ICartInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                cart.Amount -= amount;
                cart.IsPayed = true;
                await statefulProxy.UpdateCart(cart);

            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error {ex.Message}");
            }
        }

        private async Task<Customer> GetCustomerByIdService(int customerId)
        {
            // Invoke another service to get carts by customer
            try
            {
                var partitionId = customerId % 3;
                var statefulProxy = ServiceProxy.Create<ICustomerInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CustomersService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                var carts = await statefulProxy.GetCustomerById(customerId);

                return carts;
            }
            catch (Exception ex)
            {
                // Handle exception
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error getting carts by customer: {ex.Message}");
                return null;
            }
        }

        private async Task UpdateCustomerService(Customer customer, int bill)
        {
            try
            {
                var partitionId = customer.Id % 3;
                var statefulProxy = ServiceProxy.Create<ICustomerInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CustomersService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));
                customer.Wallet -= bill;
                await statefulProxy.UpdateCustomer(customer);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error: {ex.Message}");
            }
        }

        private async Task<Product> GetProductByIdService(string productId)
        {
            try
            {
                // Calculate partition ID based on product ID
                var statelessProxy = ServiceProxy.Create<IProductInterface>(
                new Uri("fabric:/NewServiceFabricApplication/ProductsService"));

                // Invoke the GetProductById method
                var product = await statelessProxy.GetProductById(productId);

                return product;
            }
            catch (Exception ex)
            {
                // Handle exception
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error getting product by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners(); ;
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
