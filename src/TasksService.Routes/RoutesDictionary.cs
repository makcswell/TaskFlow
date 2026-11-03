namespace TasksService.Routes;

/// <summary>
/// Routes model.
/// </summary>
public static class RoutesDictionary
{
    /// <summary>
    /// Service ID route (empty for base controller path).
    /// </summary>
    public const string ServiceId = "";

    /// <summary>
    /// Auth constants.
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Controller route.
        /// </summary>
        public const string Controller = "auth";

        /// <summary>
        /// Get token route.
        /// </summary>
        public const string GetToken = "token";

        /// <summary>
        /// Validate auth route.
        /// </summary>
        public const string ValidateAuth = "validate-auth";

        /// <summary>
        /// Validate token route.
        /// </summary>
        public const string ValidateToken = "validate-token";
    }

    /// <summary>
    /// Tasks constants.
    /// </summary>
    public static class Tasks
    {
        /// <summary>
        /// Controller route.
        /// </summary>
        public const string Controller = "tasks";
    }

    /// <summary>
    /// TaskFlow startup constants.
    /// </summary>
    public static class TaskFlowStartup
    {
        /// <summary>
        /// Base address.
        /// </summary>
        public const string BaseAddress = "/tasks";

        /// <summary>
        /// Service description.
        /// </summary>
        public const string Description = "TaskFlow task management service";
    }
}