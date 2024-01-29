using AutoMapper;
using Azure;
using Binder_Cart.Data;
using Binder_Cart.Dtos;
using Binder_Cart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binder_Cart.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuthenticationController> _logger;
        private IMapper _mapper;
        private ResponseDto _response;
        private IWebHostEnvironment _hostingEnv;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProductController(ApplicationDbContext dbContext,
            ILogger<AuthenticationController> logger,
              IWebHostEnvironment hostingEnv,
              UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _db = dbContext;
            _logger = logger;
            _mapper = mapper;
            _response = new ResponseDto();
            _hostingEnv = hostingEnv;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            try
            {
                //IEnumerable<Product> objList = await _db.Products.Include(brands => brands.Brand).Include(cat => cat.Category).ToListAsync(); 
                //_response.Result = _mapper.Map<IEnumerable<ProductDto>>(objList);
                _response.Result = await _db.Products.Include(brands => brands.Brand).Include(cat => cat.Category).ToListAsync();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                Product obj = _db.Products.First(u => u.Id == id);
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost] 
        public async Task<ResponseDto> Post([FromBody] ProductDto ProductDto)
        {
            try
            {

                var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                string userId = user.Id.ToString();

                Product product = _mapper.Map<Product>(ProductDto);
                // category.CategoryImageLocalPath = GetActualpath(CategoryDto.CategoryImageUrl);
                product.CreatedDate = DateTime.Now;
                product.UpdatedDate = DateTime.Now;
                product.CreatedId = userId;
                product.UpdatedId = userId;
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
               

                //if (ProductDto.ProductImage != null)
                //{

                //    string fileName = product.Id + Path.GetExtension(ProductDto.ProductImage.FileName);
                //    string filePath = @"wwwroot\ProductImages\" + fileName;

                //    //I have added the if condition to remove the any image with same name if that exist in the folder by any change
                //    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                //    FileInfo file = new FileInfo(directoryLocation);
                //    if (file.Exists)
                //    {
                //        file.Delete();
                //    }

                //    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                //    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                //    {
                //        ProductDto.ProductImage.CopyTo(fileStream);
                //    }
                //    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                //    product.ProductImageUrl = baseUrl + "/ProductImages/" + fileName;
                //    product.ProductImageLocalPath = filePath;
                //}
                //else
                //{
                //    product.ProductImageUrl = "https://placehold.co/600x400";
                //}
                //_db.Products.Update(product);
                //_db.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public ResponseDto Put(ProductDto ProductDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(ProductDto);

                if (ProductDto.ProductImage != null)
                {
                    if (!string.IsNullOrEmpty(product.ProductImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ProductImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = product.Id + Path.GetExtension(ProductDto.ProductImage.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        ProductDto.ProductImage.CopyTo(fileStream);
                    }
                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    product.ProductImageUrl = baseUrl + "/ProductImages/" + fileName;
                    product.ProductImageLocalPath = filePath;
                }


                _db.Products.Update(product);
                _db.SaveChanges();

                _response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ResponseDto> Delete(int id)
        {
            try
            {
                Product obj = await _db.Products.FirstAsync(u => u.Id == id);
                if (!string.IsNullOrEmpty(obj.ProductImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), obj.ProductImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                _db.Products.Remove(obj);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("UploadImage")]
        public async Task<ActionResult> UploadImage()
        {
            bool Result = false;
            var Files = Request.Form.Files;
            foreach (IFormFile source in Files)
            {
                string FileName = source.FileName;
                string imagepath = GetActualpath(FileName);
                try
                {

                    string Filepath = imagepath;

                    if (System.IO.File.Exists(Filepath))
                        System.IO.File.Delete(Filepath);

                    //using (FileStream stream = System.IO.File.Create(Filepath))
                    //{
                    // await   source.CopyToAsync(stream);
                    //    Result = true;
                    //}

                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), Filepath);
                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        await source.CopyToAsync(fileStream);
                        Result = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Ok(Result);
        }

        [NonAction]
        public string GetActualpath(string FileName)
        {
            return _hostingEnv.WebRootPath + "\\Jithesh\\Project Files\\Test\\ASP.NET\\Binder-Cart\\ClientApp\\BinderUI\\src\\assets\\Uploads\\Product\\" + FileName;

        }
    }
}
