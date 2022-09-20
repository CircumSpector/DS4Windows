using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Core.DependencyInjection;

/// <summary>
///     Extends the host builder components.
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    ///     Adds a singleton implementation to the <see cref="IServiceCollection" />,
    ///     registered as the additional type definitions passed.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="services"></param>
    /// <param name="definitions"></param>
    public static void AddSingletons<TImplementation>(this IServiceCollection services, params Type[] definitions)
        where TImplementation : class
    {
        // add the singleton for the implementation
        services.AddSingleton<TImplementation>();

        // each each definition passed
        foreach (var def in definitions)
            services.AddSingleton(def, s => s.GetService<TImplementation>());
    }

    /// <summary>
    ///     Adds the specified implementation to the <see cref="IServiceCollection" />,
    ///     registered as the additional type definitions passed.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementation"></param>
    /// <param name="definitions"></param>
    public static void AddSingletons(this IServiceCollection services, object implementation, params Type[] definitions)
    {
        // add the singleton for the implementation
        services.AddSingleton(implementation);

        // each each definition passed
        foreach (var def in definitions)
            services.AddSingleton(def, s => s.GetService(implementation.GetType()));
    }

    /// <summary>
    ///     Adds a transient implementation to the <see cref="IServiceCollection" />,
    ///     registered as the additional type definitions passed.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="services"></param>
    /// <param name="definitions"></param>
    public static void AddTransients<TImplementation>(this IServiceCollection services, params Type[] definitions)
        where TImplementation : class
    {
        // add the transient for the implementation
        services.AddTransient<TImplementation>();

        // each each definition passed
        foreach (var def in definitions)
            services.AddTransient(def, s => s.GetService<TImplementation>());
    }
}