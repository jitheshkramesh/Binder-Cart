using AutoMapper;
using Binder_Cart.Data;
using Binder_Cart.Dtos;
using Binder_Cart.Models;
using Binder_Cart.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Binder_Cart.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuthenticationController> _logger;
        private IMapper _mapper;
        private ResponseDto _response;
        private readonly UserManager<ApplicationUser> _userManager;
        public CartController(ApplicationDbContext dbContext,
            ILogger<AuthenticationController> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _db = dbContext;
            _logger = logger;
            _mapper = mapper;
            _response = new ResponseDto();
            _userManager = userManager;
        }

        [HttpGet("GetCart")]
        public async Task<ResponseDto> GetCart()
        {
            try
                {
                var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                string userId = user.Id.ToString(); 
             var query =(from c in _db.CartDetails
                                    join h in _db.CartHeaders on c.CartHeaderId equals h.CartHeaderId
                         join p in _db.Products on c.ProductId equals p.Id
                         join ba in _db.Brands on p.Brand.Id equals ba.Id
                         join ca in _db.Categories on p.Category.Id equals ca.Id
                         where (h.UserId == userId)
                                    select new
                                    {
                                        CartDetailId=c.CartDetailsId,
                                        CartHeaderId=h.CartHeaderId,
                                        ProductId=c.ProductId,
                                        Count=c.Count,
                                        ProductName= p.ProductName,
                                        ProductImageUrl=p.ProductImageUrl,
                                        ProductPrice= p.ProductPrice,
                                        CategoryName= ca.CategoryName,
                                        CategoryImageUrl= ca.CategoryImageUrl,
                                        BrandName= ba.BrandName,
                                        BrandImageUrl= ba.BrandImageUrl
                                    });
                _response.Result =await query.ToListAsync();
                _response.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    _response.IsSuccess = false;
                    _response.Message = ex.Message;
                }
                return _response; 
        }

        [HttpPost("AddToCart")]
        //[Authorize(Roles = UserRoles.Admin)]
        public async Task<ResponseDto> AddToCart([FromBody] CartUpdate cartUpdate)
        {
            try
            { 
                int productId = cartUpdate.productId;
                int qty = cartUpdate.quantity;
                var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                string userId = user.Id.ToString();

              //  string userId = "ebe3c285-b089-4402-9344-83f503170735";


                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                   .FirstOrDefaultAsync(u => u.UserId == userId);
                if (cartHeaderFromDb == null)
                {
                    CartHeaderDto cartHeaderDto = new CartHeaderDto() { UserId = userId };
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartHeaderDto);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();

                    CartDetailsDto cartDetailsDto = new CartDetailsDto()
                    {
                        CartHeaderId = cartHeader.CartHeaderId,
                        ProductId = productId,
                        Count = qty
                    };

                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);
                    _db.CartDetails.Add(cartDetails);
                    await _db.SaveChangesAsync();
                }
                else
                {

                    var cartHeaderFromDbs = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(
                    u => u.UserId == userId);

                    //if header is not null
                    //check if details has same product
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        u => u.ProductId == productId &&
                        u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        //create cartdetails cartHeaderFromDb.CartHeaderId
                        CartDetailsDto cartDetailsDto = new CartDetailsDto()
                        {
                            CartHeaderId = cartHeaderFromDbs.CartHeaderId,
                            ProductId = productId,
                            Count = qty
                        };

                        CartDetails cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);
                        _db.CartDetails.Add(cartDetails);
                         
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update count in cart details
                        cartDetailsFromDb.Count += cartDetailsFromDb.Count; 
                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDetailsFromDb));
                        await _db.SaveChangesAsync();
                    }
                }
                _response.Result = cartHeaderFromDb;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        //[HttpPost("CartUpsert")]
        //public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        //{
        //    try
        //    {
        //        var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
        //            .FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
        //        if (cartHeaderFromDb == null)
        //        {
        //            //create header and details
        //            CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
        //            _db.CartHeaders.Add(cartHeader);
        //            await _db.SaveChangesAsync();
        //            cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
        //            _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
        //            await _db.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            //if header is not null
        //            //check if details has same product
        //            var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
        //                u => u.ProductId == cartDto.CartDetails.First().ProductId &&
        //                u.CartHeader.CartHeaderId == cartHeaderFromDb.CartHeaderId);
        //            if (cartDetailsFromDb == null)
        //            {
        //                //create cartdetails
        //                cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
        //                _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
        //                await _db.SaveChangesAsync();
        //            }
        //            else
        //            {
        //                //update count in cart details
        //                cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
        //                cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeader.CartHeaderId;
        //                cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
        //                _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
        //                await _db.SaveChangesAsync();
        //            }
        //        }
        //        _response.Result = cartDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.Message = ex.Message.ToString();
        //        _response.IsSuccess = false;
        //    }
        //    return _response;
        //}



        //[HttpPost("RemoveCart")]
        //public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        //{
        //    try
        //    {
        //        CartDetails cartDetails = _db.CartDetails
        //           .First(u => u.CartDetailsId == cartDetailsId);

        //        int totalCountofCartItem = _db.CartDetails.Where(u => u.CartHeader.CartHeaderId == cartDetails.CartHeader.CartHeaderId).Count();
        //        _db.CartDetails.Remove(cartDetails);
        //        if (totalCountofCartItem == 1)
        //        {
        //            var cartHeaderToRemove = await _db.CartHeaders
        //               .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeader.CartHeaderId);

        //            _db.CartHeaders.Remove(cartHeaderToRemove);
        //        }
        //        await _db.SaveChangesAsync();

        //        _response.Result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.Message = ex.Message.ToString();
        //        _response.IsSuccess = false;
        //    }
        //    return _response;
        //}
    }
}
