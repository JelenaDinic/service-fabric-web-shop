using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Communication;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ProductsService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed class ProductsService : StatelessService, IProductInterface
    {
        public ProductsService(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<List<Product>> GetAllProducts()
        {
            List<Product> products = new List<Product>();
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;");
                CloudTable _table;
                CloudTableClient tableClient = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri), storageAccount.Credentials);
                _table = tableClient.GetTableReference("Products");

                TableQuery<ProductEntity> query = new TableQuery<ProductEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Electronics")
                );

                TableContinuationToken token = null;
                do
                {
                    var resultSegment = await _table.ExecuteQuerySegmentedAsync(query, token);
                    token = resultSegment.ContinuationToken;

                    foreach (ProductEntity productEntity in resultSegment.Results)
                    {
                        products.Add(new Product(productEntity.RowKey, productEntity.PartitionKey, productEntity.Name, productEntity.Description, productEntity.Amount, productEntity.Price));
                    }
                } while (token != null);
            }
            catch (Exception e)
            {
                string err = e.Message;
                ServiceEventSource.Current.Message(err);
            }
            return products;
        }

        public async Task<Product> GetProductById(string productId)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;");
                CloudTable _table;
                CloudTableClient tableClient = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri), storageAccount.Credentials);
                _table = tableClient.GetTableReference("Products");

                TableOperation retrieveOperation = TableOperation.Retrieve<ProductEntity>("Electronics", productId);
                TableResult result = await _table.ExecuteAsync(retrieveOperation);

                if (result.Result != null)
                {
                    ProductEntity productEntity = (ProductEntity)result.Result;
                    return new Product(productEntity.RowKey, productEntity.PartitionKey, productEntity.Name, productEntity.Description, productEntity.Amount, productEntity.Price);
                }
            }
            catch (Exception e)
            {
                string err = e.Message;
                ServiceEventSource.Current.Message(err);
            }

            return null; // Return null if the product is not found or an exception occurs
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

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
