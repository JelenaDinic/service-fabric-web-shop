using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    [DataContract]
    public class Cart
    {
        public Cart()
        { }
        public Cart(int id, int customerId, string productId, int amount)
        {
            Id = id;
            CustomerId = customerId;
            ProductId = productId;
            Amount = amount;
        }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
        [DataMember]
        public string ProductId { get; set; }
        [DataMember]
        public int Amount { get; set; }
        [DataMember]
        public bool IsPayed {  get; set; }
    }
}
