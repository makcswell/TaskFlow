namespace TasksService.Routes;
/// <summary>
/// Routes model
/// </summary>
public static class RoutesDictionary
{
    /// <summary>
    /// auth constant models
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Controller 
        /// </summary>
        public const string Controller = "auth";
        /// <summary>
        /// Token
        /// </summary>
        public const string GetToken = "token";
        /// <summary>
        /// ValidateAuth
        /// </summary>
        public const string ValidateAuth = "validate-auth";
        
    }        

    /// <summary>
    /// Tasks constant models
    /// </summary>
    public static class Tasks
    {
        /// <summary>
        /// Controller task model
        /// </summary>
        public const string Controller = "tasks";
        /// <summary>
        /// GetById Task model
        /// </summary>
        public const string GetById = "{id:guid}";
    }
}