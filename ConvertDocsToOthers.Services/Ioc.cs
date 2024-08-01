using ConvertDocsToOthers.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConvertDocsToOthers.Services
{
    public static class Ioc
    {
        public static void AddServicesRegistry(this IServiceCollection services)
        {
            services.AddScoped<IConvertFiles, ConvertFiles>();
        }
    }
}
