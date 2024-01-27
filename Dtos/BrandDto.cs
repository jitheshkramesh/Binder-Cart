namespace Binder_Cart.Dtos
{
    public class BrandDto
    {
        public int? Id { get; set; }
        public required string BrandName { get; set; }
        public string? BrandImageUrl { get; set; }
        public string? BrandImageLocalPath { get; set; }
        public IFormFile? BrandImage { get; set; }
    }
}
