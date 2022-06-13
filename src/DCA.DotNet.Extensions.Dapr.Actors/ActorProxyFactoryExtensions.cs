namespace Dapr.Actors.Client;

public static class ActorProxyFactoryExtensions
{
    public static TActorInterface CreateNamespacedActorProxy<TActorInterface>(
        this IActorProxyFactory factory,
        ActorId actorId,
        string @namespace,
        string actorType,
        ActorProxyOptions? options = null)
        where TActorInterface : IActor
    {
        return factory.CreateActorProxy<TActorInterface>(
            actorId,
            $"{@namespace}:{actorType}",
            options);
    }
}
