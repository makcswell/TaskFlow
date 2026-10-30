namespace TasksService.Models;

///<summary>
///TaskItem DB Models
///</summary>
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
    
}