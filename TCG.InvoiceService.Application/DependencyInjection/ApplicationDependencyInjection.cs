using System.Reflection;
using GreenPipes;
using Mapster;
using MapsterMapper;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TCG.Common.MassTransit.Messages;
using TCG.Common.Settings;

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
            configure.UsingRabbitMq((context, configurator) =>
            {
                var config = context.GetService<IConfiguration>();
                ////On recupère la config de seeting json pour rabbitMQ
                var rabbitMQSettings = config.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(new Uri(rabbitMQSettings.Host));
                configurator.ConfigureEndpoints(context);
                // Retry policy for consuming messages
                configurator.UseMessageRetry(retryConfig =>
                {
                    // Exponential back-off (second argument is the max retry count)
                    retryConfig.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
                });
                
                // // Message Redelivery/Dead-lettering
                // configurator.UseScheduledRedelivery(r => r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));
                // configurator.ReceiveEndpoint("my-dead-letter-queue", e =>
                // {
                //     e.ConfigureConsumer<DeadLetterEventConsumer>(context);
                // });
                
                //Defnir comment les queues sont crées dans rabbit
                configurator.ConfigureEndpoints(context);
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