﻿using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Events.EventHandlers;
using EventBus.UnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.UnitTest
{
    [TestClass]
    public class EventBusTest
    {
        private ServiceCollection services;

        public EventBusTest()
        {
            services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());

        }

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
               
                return EventBusFactory.Create(getrabbitmqconfig(), sp);
            });
            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

        }

        [TestMethod]
        public void subscribe_event_on_azure_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(getazureconfig(), sp);
            });
            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            Task.Delay(2000).Wait();

        }


        [TestMethod]
        public void send_message_to_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {

                return EventBusFactory.Create(getrabbitmqconfig(), sp);
            });
            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(2));
        }
        
        [TestMethod]
        public void send_message_to_azure_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {

                return EventBusFactory.Create(getazureconfig(), sp);
            });
            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(2));
        }


        
        private EventBusConfig getazureconfig()
        {
            return new EventBusConfig
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "OnSaleTopicName",
                EventBusType = EventBusType.AzureServiceBus,
                EventNameSuffix = "IntegrationEvent",
                EventBusConnectionString = "AZURE-ENDPOINT"

            };
        }
        
        private EventBusConfig getrabbitmqconfig()
        {
            return new EventBusConfig
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "OnSaleTopicName",
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent"
                //Connection = new ConnectionFactory() // It is already configurated by default configs
                //{
                //    HostName = "localhost",
                //    Port = 5672,
                //    UserName = "guest",
                //    Password = "guest"
                //}
            };
        }
    }
}