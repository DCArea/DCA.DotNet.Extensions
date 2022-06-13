using System.Reflection;

namespace Dapr.Actors.Runtime;

public static class ActorRegistrationCollectionExtensions
{
    public static void RegisterNamespacedActor<TActor>(
        this ActorRegistrationCollection collection,
        string @namespace!!,
        Action<ActorRegistration>? configure = null)
        where TActor : Actor
    {
        RegisterNamespacedActor<TActor>(collection, @namespace, actorTypeName: null, configure);
    }

    public static void RegisterNamespacedActor<TActor>(
        this ActorRegistrationCollection collection,
        string @namespace!!,
        string? actorTypeName,
        Action<ActorRegistration>? configure = null)
        where TActor : Actor
    {
        var actorType = typeof(TActor);
        var actorAttribute = actorType.GetCustomAttribute<ActorAttribute>();
        actorTypeName ??= actorAttribute?.TypeName ?? actorType.Name;
        if(actorTypeName.Contains(':'))
        {
            throw new ArgumentException($"{nameof(actorTypeName)} cannot contain ':'");
        }
        string namespacedActorTypeName = $"{@namespace}:{actorTypeName}";
        var actorTypeInfo = ActorTypeInformation.Get(actorType, namespacedActorTypeName);
        var registration = new ActorRegistration(actorTypeInfo);
        configure?.Invoke(registration);
        collection.Add(registration);
    }
}
