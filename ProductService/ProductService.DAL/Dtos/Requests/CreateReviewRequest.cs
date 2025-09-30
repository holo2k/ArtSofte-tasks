namespace ProductService.DAL.Dtos.Requests;

public class CreateReviewRequest
{
    public int Rating { get; set; }
    public string Text { get; set; } = "";
}