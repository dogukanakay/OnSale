using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using PaymentService.Api.IntegrationEvents.Events;

namespace NotificationService.IntegrationEvents.EventHandlers
{
    class OrderPaymentSuccessIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
    {
        private readonly ILogger<OrderPaymentSuccessIntegrationEventHandler> _logger;

        public OrderPaymentSuccessIntegrationEventHandler(ILogger<OrderPaymentSuccessIntegrationEventHandler> logger)
        {
            _logger = logger;
        }
        public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
        {
            // Send Fail Notification (Sms, EMail, Push)

           _logger.LogInformation($"Order Payment Success with OrderId: {@event.OrderId}");

            return Task.CompletedTask;
        }
    }
}
