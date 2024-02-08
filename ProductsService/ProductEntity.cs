using Microsoft.WindowsAzure.Storage.Table;

namespace ProductsService
{
    public class ProductEntity : TableEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
    }
}
