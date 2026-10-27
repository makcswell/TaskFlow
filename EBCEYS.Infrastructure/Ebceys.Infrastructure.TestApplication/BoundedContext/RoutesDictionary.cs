namespace Ebceys.Infrastructure.TestApplication.BoundedContext;

public static class RoutesDictionary
{
    public const string BasePath = "/test-app";

    public static class TestControllerV1
    {
        internal const string ControllerRoute = "api/v{version:apiVersion}/[controller]";
        internal const string ApiVersion = "1.0";

        public const string BaseRoute = BasePath + "/api/v1/test";

        public class Methods
        {
            public const string GetOk = "ok";
            public const string GetJson = "json";
            public const string GetException = "exception";
            public const string PostBody = "body";
            public const string GetQuery = "put";
            public const string PostCommand = "command";
            public const string GetCommand = "command";
            public const string PutCommand = "command";
            public const string DeleteCommand = "command";

            public const string GetToken = "token";
            public const string ValidateToken = "validate/token";
            public const string ValidateAuth = "validate/auth";
        }
    }
}