using AbaDeskBack.DTOs.Users;
using System;

namespace AbaDeskBack.DTOs.Comments;

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public UserResponse User { get; set; } = null!;
}
