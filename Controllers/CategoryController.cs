﻿using AutoMapper;
using Binder_Cart.Data;
using Binder_Cart.Dtos;
using Binder_Cart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private IWebHostEnvironment _hostingEnv;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoryController(ApplicationDbContext dbContext,
            ILogger<AuthenticationController> logger,
            IMapper mapper,
            IWebHostEnvironment hostingEnv,
            UserManager<ApplicationUser> userManager)
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
                IEnumerable<Category> objList =await _db.Categories.OrderByDescending(o => o.Id).ToListAsync();
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
        public async Task<ResponseDto> Get(int id)
        {
            try
            {
                Category obj = await _db.Categories.FirstAsync(u => u.Id == id);
                _response.Result = _mapper.Map<CategoryDto>(obj);
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
            return _hostingEnv.WebRootPath + "\\Jithesh\\Project Files\\Test\\ASP.NET\\Binder-Cart\\ClientApp\\BinderUI\\src\\assets\\Uploads\\Category\\" + FileName;

        }

        [HttpPost] 
        public async Task<ResponseDto> Post([FromBody] CategoryDto CategoryDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                string userId = user.Id.ToString();

                var categoryFromDb = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(
                        u => u.Id == CategoryDto.Id);
                if (categoryFromDb == null)
                {

                    Category category = _mapper.Map<Category>(CategoryDto);
                    // category.CategoryImageLocalPath = GetActualpath(CategoryDto.CategoryImageUrl);
                    category.CreatedDate = DateTime.Now;
                    category.UpdatedDate = DateTime.Now;
                    category.CreatedId = userId;
                    category.UpdatedId = userId;
                    _db.Categories.Add(category);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    categoryFromDb.CategoryName = CategoryDto.CategoryName;
                    categoryFromDb.CategoryImageUrl = CategoryDto.CategoryImageUrl;
                    _db.Categories.Update(categoryFromDb);
                    await _db.SaveChangesAsync();
                }
                _response.Result = _mapper.Map<CategoryDto>(categoryFromDb);
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

        [HttpPatch("checkProto")]
        public async Task<ResponseDto> checkProto([FromBody] CategoryDto CategoryDto)
        {
            try
            {
               // var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                string userId = "07148931-4870-42b5-9094-9c85c1b3a026";//user.Id.ToString();

                var categoryFromDb = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(
                        u => u.Id == CategoryDto.Id);
                if (categoryFromDb == null)
                {

                    Category category = _mapper.Map<Category>(CategoryDto);
                    // category.CategoryImageLocalPath = GetActualpath(CategoryDto.CategoryImageUrl);
                    category.CreatedDate = DateTime.Now;
                    category.UpdatedDate = DateTime.Now;
                    category.CreatedId = userId;
                    category.UpdatedId = userId;
                    _db.Categories.Add(category);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    categoryFromDb.CategoryName = CategoryDto.CategoryName;
                    categoryFromDb.CategoryImageUrl = CategoryDto.CategoryImageUrl;
                    _db.Categories.Update(categoryFromDb);
                    await _db.SaveChangesAsync();
                }
                _response.Result = _mapper.Map<CategoryDto>(categoryFromDb);
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
