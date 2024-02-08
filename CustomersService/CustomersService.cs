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

namespace CustomersService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CustomersService : StatefulService, ICustomerInterface
    {
        public CustomersService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<Customer> GetCustomerById(int id)
        {
            var stateManager = this.StateManager;

            var customerDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Customer>>("customerdict");

            using (var tx = stateManager.CreateTransaction())
            {
                var product = await customerDict.TryGetValueAsync(tx, id);

                return product.Value;
            }

            throw new Exception();
        }

        public async Task AddCustomer(Customer customer)
        {
            var stateManager = this.StateManager;

            var customerDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Customer>>("customerdict");

            using (var tx = stateManager.CreateTransaction())
            {
                await customerDict.AddOrUpdateAsync(tx, customer.Id, customer, (k, v) => v);

                await tx.CommitAsync();
            }
        }

        public async Task<int> Login(string email, string password)
        {
            var stateManager = this.StateManager;
            var customerDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Customer>>("customerdict");

            using (var tx = stateManager.CreateTransaction())
            {
                var enumerable = await customerDict.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    var storedCustomer = enumerator.Current.Value;

                    if (storedCustomer.Email == email && storedCustomer.Password == password)
                    {
                        return storedCustomer.Id;
                    }
                }

                return 0;
            }
        }

        public async Task UpdateCustomer(Customer customer)
        {
            var stateManager = this.StateManager;

            var customerDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Customer>>("customerdict");

            using (var tx = stateManager.CreateTransaction())
            {
                await customerDict.SetAsync(tx, customer.Id, customer);

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
