using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PointController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetAll")]
        public async Task<ApiResponse<List<PointE>>> GetAll()
        {
            try
            {
                var points = await _unitOfWork.Points.GetAllAsync();
                return new ApiResponse<List<PointE>>
                {
                    Success = true,
                    Data = points.ToList()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PointE>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ApiResponse<PointE>> GetById(int id)
        {
            try
            {
                var point = await _unitOfWork.Points.GetByIdAsync(id);
                if (point == null)
                {
                    return new ApiResponse<PointE>
                    {
                        Success = false,
                        Message = "Point not found"
                    };
                }

                return new ApiResponse<PointE>
                {
                    Success = true,
                    Data = point
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PointE>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("UpdateById/{id}")]
        public async Task<ApiResponse<PointE>> UpdateById(int id, [FromBody] UpdatePointRequest request)
        {
            try
            {
                var existingPoint = await _unitOfWork.Points.GetByIdAsync(id);
                if (existingPoint == null)
                {
                    return new ApiResponse<PointE>
                    {
                        Success = false,
                        Message = "Point not found"
                    };
                }

                existingPoint.Name = request.NewName;
                existingPoint.Geometry = request.NewGeometry;

                await _unitOfWork.Points.UpdateAsync(existingPoint);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<PointE>
                {
                    Success = true,
                    Data = existingPoint
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PointE>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("DeleteById/{id}")]
        public async Task<ApiResponse<PointE>> DeleteById(int id)
        {
            try
            {
                var point = await _unitOfWork.Points.GetByIdAsync(id);
                if (point == null)
                {
                    return new ApiResponse<PointE>
                    {
                        Success = false,
                        Message = "Point not found"
                    };
                }

                await _unitOfWork.Points.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<PointE>
                {
                    Success = true,
                    Data = point
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PointE>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("Add")]
        public async Task<ApiResponse<PointE>> Add([FromBody] AddPointDto pointDto)
        {
            try
            {
                var point = new PointE
                {
                    Name = pointDto.Name,
                    Geometry = pointDto.Geometry
                };

                await _unitOfWork.Points.AddAsync(point);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<PointE>
                {
                    Success = true,
                    Data = point
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PointE>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("AddRange")]
        public async Task<ApiResponse<List<PointE>>> AddRange([FromBody] List<AddPointDto> pointDtos)
        {
            try
            {
                var points = new List<PointE>();

                foreach (var dto in pointDtos)
                {
                    var point = new PointE
                    {
                        Name = dto.Name,
                        Geometry = dto.Geometry
                    };

                    await _unitOfWork.Points.AddAsync(point);
                    points.Add(point);
                }

                await _unitOfWork.SaveAsync();

                return new ApiResponse<List<PointE>>
                {
                    Success = true,
                    Data = points
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PointE>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }

    public class UpdatePointRequest
    {
        public string NewName { get; set; } = string.Empty;
        public Geometry NewGeometry { get; set; }
    }
}