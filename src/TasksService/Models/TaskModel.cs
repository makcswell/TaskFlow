namespace TasmModel;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Status { get; set; } = "New";
    public string UserId { get; set; } = string.Empty;
    
}