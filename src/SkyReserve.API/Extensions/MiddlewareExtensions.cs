namespace SkyReserve.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Middleware.GlobalExceptionHandlingMiddleware>();
        }
    }
}