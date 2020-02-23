using Microsoft.AspNetCore.Builder;

namespace NextPipe.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionFilder(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
        
        public static IApplicationBuilder UseDefaultNextPipeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseGlobalExceptionFilder();
        }
    }
}