using Microsoft.Extensions.DependencyInjection;

namespace kinohannover.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static void AddServicesByInterface<T>(this IServiceCollection services)
        {
            var servicesThatImplementInterface = typeof(T).Assembly.GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(T)) && !t.IsInterface);

            foreach (var service in servicesThatImplementInterface)
            {
                services.AddScoped(typeof(T), service);
            }
        }
    }
}