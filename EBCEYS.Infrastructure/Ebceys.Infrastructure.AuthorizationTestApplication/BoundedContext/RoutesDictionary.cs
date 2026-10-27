namespace Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;

public static class RoutesDictionary
{
    public const string BasePath = "/auth-service";
    public const string ServiceName = "auth-service";

    public static class AuthControllerV1
    {
        internal const string ControllerRoute = "api/v{version:apiVersion}/[controller]";
        internal const string ApiVersion = "1.0";

        public const string BaseRoute = BasePath + "/api/v1/auth";

        public class Methods
        {
            public const string GetToken = "token";
            public const string ValidateToken = "validate/token";
            public const string ValidateAuth = "validate/auth";
        }
    }

    public static class RabbitMqControllerV1
    {
        public static class Methods
        {
            public const string GetOk = "ok";
            public const string GetJson = "getjson";
        }
    }
}