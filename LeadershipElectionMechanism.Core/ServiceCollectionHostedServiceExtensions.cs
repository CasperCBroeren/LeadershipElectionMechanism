using Microsoft.Extensions.DependencyInjection; 

namespace LeadershipElectionMechanism.Core.Extensions
{
    public static class ServiceCollectionHostedServiceExtensions
    { 
        public static IServiceCollection AddLeadershipWork<TLeaderWork>(this IServiceCollection services, Action<LeadershipOptions> configureOptions)
            where TLeaderWork : class, ILeaderWork
        { 
            services.AddSingleton<TLeaderWork>(); 
            services.AddHostedService(provider =>
            {
                var work = provider.GetRequiredService<TLeaderWork>();
                return new HostedServiceLeaderWorker<TLeaderWork>(work, configureOptions, work.GetType().Name);
            }); 
            return services;
        } 
    }

}
