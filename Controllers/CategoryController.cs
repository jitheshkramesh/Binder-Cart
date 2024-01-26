using AutoMapper;
using Binder_Cart.Data;
using Binder_Cart.Dtos;
using Binder_Cart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binder_Cart.Controllers
{
    [Route("api/Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuthenticationController> _logger;
        private IMapper _mapper;
        private ResponseDto _response;
        public CategoryController(ApplicationDbContext dbContext,
            ILogger<AuthenticationController> logger,
            IMapper mapper)
        {
            _db = dbContext;
            _logger = logger;
            _mapper = mapper;
            _response = new ResponseDto();
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Category> objList = _db.Categories.ToList();
                _response.Result = _mapper.Map<IEnumerable<CategoryDto>>(objList);
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
                Category obj = _db.Categories.First(u => u.Id == id);
                _response.Result = _mapper.Map<CategoryDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public ResponseDto Post(CategoryDto CategoryDto)
        {
            try
            {
                Category category = _mapper.Map<Category>(CategoryDto);
                _db.Categories.Add(category);
                _db.SaveChanges();

                if (CategoryDto.CategoryImageUrl != null)
                {

                    string fileName = category.Id + Path.GetExtension(CategoryDto.CategoryImage.FileName);
                    string filePath = @"wwwroot\CategoryImages\" + fileName;

                    //I have added the if condition to remove the any image with same name if that exist in the folder by any change
                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    FileInfo file = new FileInfo(directoryLocation);
                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        CategoryDto.CategoryImage.CopyTo(fileStream);
                    }
                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    category.CategoryImageUrl = baseUrl + "/CategoryImages/" + fileName;
                    category.CategoryImageLocalPath = filePath;
                }
                else
                {
                    category.CategoryImageUrl = "https://placehold.co/600x400";
                }
                _db.Categories.Update(category);
                _db.SaveChanges();
                _response.Result = _mapper.Map<CategoryDto>(category);
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
        public ResponseDto Put(CategoryDto categoryDto)
        {
            try
            {
                Category category = _mapper.Map<Category>(categoryDto);

                if (categoryDto.CategoryImage != null)
                {
                    if (!string.IsNullOrEmpty(category.CategoryImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), category.CategoryImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = category.Id + Path.GetExtension(categoryDto.CategoryImage.FileName);
                    string filePath = @"wwwroot\CategoryImages\" + fileName;
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        categoryDto.CategoryImage.CopyTo(fileStream);
                    }
                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    category.CategoryImageUrl = baseUrl + "/CategoryImages/" + fileName;
                    category.CategoryImageLocalPath = filePath;
                }


                _db.Categories.Update(category);
                _db.SaveChanges();

                _response.Result = _mapper.Map<CategoryDto>(category);
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
        [Authorize(Roles = UserRoles.Admin)]
        public ResponseDto Delete(int id)
        {
            try
            {
                Category obj = _db.Categories.First(u => u.Id == id);
                if (!string.IsNullOrEmpty(obj.CategoryImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), obj.CategoryImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                _db.Categories.Remove(obj);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
