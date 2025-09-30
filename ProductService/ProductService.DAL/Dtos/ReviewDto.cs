namespace ProductService.DAL.Dtos;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid AuthorId { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = "";
    public string? Reply { get; set; }
    public DateTime CreatedAt { get; set; }
}