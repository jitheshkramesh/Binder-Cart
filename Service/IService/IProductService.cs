using Binder_Cart.Dtos;

namespace Binder_Cart.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
