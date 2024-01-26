namespace Binder_Cart.Dtos
{
    public class CategoryDto
    {
        public required string CategoryName { get; set; }
        public string? CategoryImageUrl { get; set; }
        public string? CategoryImageLocalPath { get; set; }
        public IFormFile? CategoryImage { get; set; }
    }
}
