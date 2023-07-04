using System.Reflection;
using GreenPipes;
using Mapster;
using MapsterMapper;
using MassTransit;
using MassTransit.Saga;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TCG.Common.MassTransit.Messages;
using TCG.Common.Settings;
using TCG.InvoiceService.Application.Order.Saga;

namespace TCG.InvoiceService.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services.AddMediatR(assembly);
    }
    
    public static IServiceCollection AddMapper(this IServiceCollection services)
    {
        var config = new TypeAdapterConfig();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
    
    public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection serviceCollection)
    {
        //Config masstransit to rabbitmq
        serviceCollection.AddMassTransit(configure =>
        {
            configure.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().InMemoryRepository();
            configure.UsingRabbitMq((context, configurator) =>
            {
                var config = context.GetService<IConfiguration>();
                ////On recupère la config de seeting json pour rabbitMQ
                var rabbitMQSettings = config.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(new Uri(rabbitMQSettings.Host));
                
                // Retry policy for consuming messages
                configurator.UseMessageRetry(retryConfig =>
                {
                    // Exponential back-off (second argument is the max retry count)
                    retryConfig.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
                });
                
                configurator.ReceiveEndpoint("order-state", e =>
                {
                    e.StateMachineSaga(context.GetService<OrderStateMachine>(), new InMemorySagaRepository<OrderStateInstance>());
                });
                // // Message Redelivery/Dead-lettering
                // configurator.UseScheduledRedelivery(r => r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));
                // configurator.ReceiveEndpoint("my-dead-letter-queue", e =>
                // {
                //     e.ConfigureConsumer<DeadLetterEventConsumer>(context);
                // });
                
                //Defnir comment les queues sont crées dans rabbit
            });
            configure.AddRequestClient<PostCreated>();
            configure.AddRequestClient<UserById>();
            configure.AddRequestClient<BuyerTransaction>();
        });
        //Start rabbitmq bus pour exanges
        serviceCollection.AddMassTransitHostedService();
        return serviceCollection;
    }
}