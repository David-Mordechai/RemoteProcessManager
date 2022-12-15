using MessageBroker.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker;

public static class ServiceCollectionExtension
{
    public static void AddMessageBroker(this IServiceCollection services, MessageBrokerSettings messageBrokerSettings)
    {
        services.AddSingleton(messageBrokerSettings);
        services.AddSingleton<IConsumer, RedisConsumer>();
        services.AddSingleton<IProducer, RedisProducer>();
    }
}