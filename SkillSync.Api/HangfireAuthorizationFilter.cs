using Hangfire.Dashboard;

namespace SkillSync.Api
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In development, allow all users to see the Hangfire dashboard
            // In production, implement proper authorization logic here

            var httpContext = context.GetHttpContext();
            return httpContext.Request.IsLocal() || httpContext.Request.Host.Host == "localhost";
        }
    }

    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                return connection.LocalIpAddress != null
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    : System.Net.IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }
    }
}
