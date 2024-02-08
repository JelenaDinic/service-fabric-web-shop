using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    [DataContract]
    public class Product
    {
        public Product(string productId, string category, string name, string description, int amount, int price)
        {
            Id = productId;
            Category = category;
            Name = name;
            Description = description;
            Amount = amount;
            Price = price;
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Amount { get; set; }
        [DataMember]
        public int Price { get; set; }
    }
}
