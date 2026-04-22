using System.Reflection;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Validation;
using Microsoft.Extensions.Hosting;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg => 
            cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddScoped<IRequestValidationService, RequestValidationService>();

        // MediatR permanece únicamente como infraestructura para publicación de eventos de dominio.
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
    }
}
