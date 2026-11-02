using JetBrains.Annotations;

namespace TaskFlow.Data;

///<summary>
///TaskItem DB Models
///</summary>
[PublicAPI]
public class TaskItem
{
    ///<summary>
    ///id
    ///</summary>
    public Guid Id { get; set; }

    ///<summary>
    ///Title
    ///</summary>
    public string Title { get; set; } = string.Empty;

    ///<summary>
    ///Description
    ///</summary>
    public string? Description { get; set; }

    ///<summary>
    ///DueDate
    ///</summary>
    public DateTimeOffset DueDate { get; set; }

    ///<summary>
    ///CreatedAt
    ///</summary>
    public DateTimeOffset CreatedAt { get; set; }

    ///<summary>
    ///Status
    ///</summary>
    public string Status { get; set; } = "New";

    ///<summary>
    ///UserId
    ///</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Запрос на генерацию токена.
    /// </summary>
    /// <param name="UserName">Имя пользователя.</param>
    public record GenerateTokenRequest(string UserName);

    /// <summary>
    /// Ответ с токеном.
    /// </summary>
    /// <param name="Token">JWT-токен.</param>
    public record GenerateTokenResponse(string Token);
}