using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public interface IPaymentInterface : IService
    {
        Task<string> ProcessPayment(int customerId);
    }
}
