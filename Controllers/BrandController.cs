using AutoMapper;
using Azure;
using Binder_Cart.Data;
using Binder_Cart.Dtos;
using Binder_Cart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binder_Cart.Controllers
{
    [Route("api/Brand")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuthenticationController> _logger;
        private IMapper _mapper;
        private ResponseDto _response;
        public BrandController(ApplicationDbContext dbContext,
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
                IEnumerable<Brand> objList = _db.Brands.ToList();
                _response.Result = _mapper.Map<IEnumerable<BrandDto>>(objList);
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
                Brand obj = _db.Brands.First(u => u.Id == id);
                _response.Result = _mapper.Map<BrandDto>(obj);
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
        public ResponseDto Post(BrandDto brandDto)
        {
            try
            {
                Brand brand = _mapper.Map<Brand>(brandDto);
                _db.Brands.Add(brand);
                _db.SaveChanges();

                if (brandDto.BrandImageUrl != null)
                {

                    string fileName = brand.Id + Path.GetExtension(brandDto.BrandImage.FileName);
                    string filePath = @"wwwroot\BrandImages\" + fileName;

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
                        brandDto.BrandImage.CopyTo(fileStream);
                    }
                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    brand.BrandImageUrl = baseUrl + "/BrandImages/" + fileName;
                    brand.BrandImageLocalPath = filePath;
                }
                else
                {
                    brand.BrandImageUrl = "https://placehold.co/600x400";
                }
                _db.Brands.Update(brand);
                _db.SaveChanges();
                _response.Result = _mapper.Map<BrandDto>(brand);
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
        public ResponseDto Put(BrandDto brandDto)
        {
            try
            {
                Brand brand = _mapper.Map<Brand>(brandDto);

                if (brandDto.BrandImage != null)
                {
                    if (!string.IsNullOrEmpty(brand.BrandImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), brand.BrandImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = brand.Id + Path.GetExtension(brandDto.BrandImage.FileName);
                    string filePath = @"wwwroot\BrandImages\" + fileName;
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        brandDto.BrandImage.CopyTo(fileStream);
                    }
                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    brand.BrandImageUrl = baseUrl + "/BrandImages/" + fileName;
                    brand.BrandImageLocalPath = filePath;
                }


                _db.Brands.Update(brand);
                _db.SaveChanges();

                _response.Result = _mapper.Map<BrandDto>(brand);
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
                Brand obj = _db.Brands.First(u => u.Id == id);
                if (!string.IsNullOrEmpty(obj.BrandImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), obj.BrandImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                _db.Brands.Remove(obj);
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
