using Microsoft.AspNetCore.Builder;

namespace Hamekoz.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHamekozMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<Middlewares.ExceptionHandlingMiddleware>();

        return app;
    }

}
