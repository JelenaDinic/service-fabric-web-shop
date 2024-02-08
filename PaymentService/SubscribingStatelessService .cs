using Communication;
using SoCreate.ServiceFabric.PubSub.Subscriber;
using SoCreate.ServiceFabric.PubSub;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService
{
    internal sealed class SubscribingStatelessService : SubscriberStatelessServiceBase
    {
        public SubscribingStatelessService(StatelessServiceContext serviceContext, IBrokerClient brokerClient = null)
            : base(serviceContext, brokerClient)
        {
        }

        [Subscribe]
        private Task HandleMessageOne(PublishedMessageOne message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing PublishedMessageOne: {message.Content}");
            return Task.CompletedTask;
        }

        [Subscribe(QueueType.Unordered)]
        private Task HandleMessageTwo(PublishedMessageTwo message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, $"Processing PublishedMessageTwo: {message.Content}");
            return Task.CompletedTask;
        }
    }
}
