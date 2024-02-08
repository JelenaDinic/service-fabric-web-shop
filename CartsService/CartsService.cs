using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CartsService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CartsService : StatefulService, ICartInterface
    {
        public CartsService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddToCart(Cart cart)
        {
            var stateManager = this.StateManager;

            var cartDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Cart>>("cartdict");

            using (var tx = stateManager.CreateTransaction())
            {
                await cartDict.AddOrUpdateAsync(tx, cart.Id, cart, (k, v) => v);

                await tx.CommitAsync();
            }
        }

        public async Task RemoveFromCart(int cartId)
        {
            var stateManager = this.StateManager;

            var cartDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Cart>>("cartdict");

            using (var tx = stateManager.CreateTransaction())
            {
                await cartDict.TryRemoveAsync(tx, cartId);

                await tx.CommitAsync();
            }
        }

        public async Task<List<Cart>> GetCartsByCustomer(int customerId)
        {
            var stateManager = this.StateManager;

            var cartDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Cart>>("cartdict");

            using (var tx = stateManager.CreateTransaction())
            {
                var result = new List<Cart>();

                var enumerable = await cartDict.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default))
                    {
                        var current = enumerator.Current;
                        if (current.Value.CustomerId == customerId)
                        {
                            result.Add(current.Value);
                        }
                    }
                }

                return result;
            }
        }

        public async Task<List<Cart>> GetCartsHistory(int customerId)
        {
            var allCarts = await GetCartsByCustomer(customerId); 
            var cartsHistory = allCarts.Where(cart => cart.IsPayed == true).ToList();

            return cartsHistory;

        }


        public async Task UpdateCart(Cart cart)
        {
            var stateManager = this.StateManager;

            var cartDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Cart>>("cartdict");

            using (var tx = stateManager.CreateTransaction())
            {
                await cartDict.AddOrUpdateAsync(tx, cart.Id, cart, (k, v) => cart);

                await tx.CommitAsync();
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
