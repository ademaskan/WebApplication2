using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Bogus;

namespace BaşarsoftStaj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShapeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShapeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("GetAll")]
        public async Task<ApiResponse<PagedResult<Shape>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var points = await _unitOfWork.Points.GetAllAsync(pageNumber, pageSize, searchTerm);
                return new ApiResponse<PagedResult<Shape>>
                {
                    Success = true,
                    Data = points
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<Shape>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ApiResponse<Shape>> GetById(int id)
        {
            try
            {
                var point = await _unitOfWork.Points.GetByIdAsync(id);
                if (point == null)
                {
                    return new ApiResponse<Shape>
                    {
                        Success = false,
                        Message = "Point not found"
                    };
                }

                return new ApiResponse<Shape>
                {
                    Success = true,
                    Data = point
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Shape>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("UpdateById/{id}")]
        public async Task<ApiResponse<Shape>> UpdateById(int id, [FromBody] UpdatePointRequest request)
        {
            try
            {
                var existingPoint = await _unitOfWork.Points.GetByIdAsync(id);
                if (existingPoint == null)
                {
                    return new ApiResponse<Shape>
                    {
                        Success = false,
                        Message = "Point not found"
                    };
                }

                existingPoint.Name = request.NewName;
                if (request.NewGeometry != null)
                {
                    existingPoint.Geometry = request.NewGeometry;
                }

                await _unitOfWork.Points.UpdateAsync(existingPoint);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<Shape>
                {
                    Success = true,
                    Data = existingPoint
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Shape>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("DeleteById/{id}")]
        public async Task<ApiResponse<Shape>> DeleteById(int id)
        {
            try
            {
                var point = await _unitOfWork.Points.GetByIdAsync(id);
                if (point == null)
                {
                    return new ApiResponse<Shape>
                    {
                        Success = false,
                        Message = "Point not found"
                    };
                }

                await _unitOfWork.Points.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<Shape>
                {
                    Success = true,
                    Data = point
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Shape>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("DeleteAll")]
        public async Task<ApiResponse<object>> DeleteAll()
        {
            try
            {
                await _unitOfWork.Points.DeleteAllAsync();
                await _unitOfWork.SaveAsync();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "All shapes deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("Add")]
        public async Task<ApiResponse<Shape>> Add([FromForm] AddPointDto pointDto, IFormFile? image)
        {
            try
            {
                var reader = new GeoJsonReader();
                var geometry = reader.Read<Geometry>(pointDto.Geometry);

                if (geometry.OgcGeometryType == OgcGeometryType.LineString)
                {
                    if (pointDto.Type == "A")
                    {
                        var intersectsWithPoints = await _unitOfWork.Points.HasIntersectingPointsAsync(geometry, new[] { "B" });
                        if (intersectsWithPoints)
                        {
                            return new ApiResponse<Shape>
                            {
                                Success = false,
                                Message = $"The new LineString of type A cannot intersect with existing Points of type B."
                            };
                        }
                    }
                }
                else if (geometry.OgcGeometryType == OgcGeometryType.Point)
                {
                    if (pointDto.Type == "B")
                    {
                        var intersectsWithLineStrings = await _unitOfWork.Points.HasIntersectingLineStringsAsync(geometry, new[] { "A" });
                        if (intersectsWithLineStrings)
                        {
                            return new ApiResponse<Shape>
                            {
                                Success = false,
                                Message = "The new Point of type B cannot intersect with existing LineStrings of type A."
                            };
                        }
                    }
                }
                
                string? imagePath = null;
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", "Images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagePath = Path.Combine("Uploads", "Images", uniqueFileName);
                }

                var point = new Shape
                {
                    Name = pointDto.Name,
                    Geometry = geometry,
                    Type = pointDto.Type,
                    ImagePath = imagePath
                };

                await _unitOfWork.Points.AddAsync(point);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<Shape>
                {
                    Success = true,
                    Data = point
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Shape>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("AddRange")]
        public async Task<ApiResponse<List<Shape>>> AddRange([FromBody] List<AddPointDto> pointDtos)
        {
            try
            {
                var points = new List<Shape>();
                var reader = new GeoJsonReader();

                foreach (var dto in pointDtos)
                {
                    var geometry = reader.Read<Geometry>(dto.Geometry);
                    if (geometry.OgcGeometryType == OgcGeometryType.LineString)
                    {
                        if (dto.Type == "A")
                        {
                            var intersectsWithPoints = await _unitOfWork.Points.HasIntersectingPointsAsync(geometry, new[] { "B" });
                            if (intersectsWithPoints)
                            {
                                return new ApiResponse<List<Shape>>
                                {
                                    Success = false,
                                    Message = $"A LineString of type A named '{dto.Name}' cannot intersect with existing Points of type B."
                                };
                            }
                        }
                    }
                    else if (geometry.OgcGeometryType == OgcGeometryType.Point)
                    {
                        if (dto.Type == "B")
                        {
                            var intersectsWithLineStrings = await _unitOfWork.Points.HasIntersectingLineStringsAsync(geometry, new[] { "A" });
                            if (intersectsWithLineStrings)
                            {
                                return new ApiResponse<List<Shape>>
                                {
                                    Success = false,
                                    Message = $"A Point of type B named '{dto.Name}' cannot intersect with existing LineStrings of type A."
                                };
                            }
                        }
                    }
                    
                    var point = new Shape
                    {
                        Name = dto.Name,
                        Geometry = geometry,
                        Type = dto.Type
                    };

                    await _unitOfWork.Points.AddAsync(point);
                    points.Add(point);
                }

                await _unitOfWork.SaveAsync();

                return new ApiResponse<List<Shape>>
                {
                    Success = true,
                    Data = points
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Shape>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("Merge")]
        public async Task<ApiResponse<Shape>> Merge([FromBody] MergeShapesRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse<Shape>
                {
                    Success = false,
                    Message = "Invalid request model."
                };
            }

            try
            {
                await _unitOfWork.Points.DeleteRangeAsync(request.DeleteIds);

                var newShape = new Shape
                {
                    Name = request.Name,
                    Geometry = request.Geometry,
                    Type = request.Type,
                };

                await _unitOfWork.Points.AddAsync(newShape);
                await _unitOfWork.SaveAsync();

                return new ApiResponse<Shape>
                {
                    Success = true,
                    Data = newShape
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Shape>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("CreateTestData/{count}")]
        public async Task<ApiResponse<object>> CreateTestData(int count)
        {
            if (count <= 0)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Count must be a positive number."
                };
            }

            try
            {
                var geometryFactory = new GeometryFactory();

                var shapeFaker = new Faker<Shape>()
                    .RuleFor(s => s.Name, f => f.Address.City())
                    .RuleFor(s => s.Type, f => f.PickRandom(new[] { "A", "B", "C" }))
                    .RuleFor(s => s.Geometry, f =>
                    {
                        var geometryType = f.PickRandom(new[] { "Point", "LineString", "Polygon" });
                        switch (geometryType)
                        {
                            case "Point":
                                return geometryFactory.CreatePoint(new Coordinate(f.Address.Longitude(), f.Address.Latitude()));
                            case "LineString":
                                var coords = new Coordinate[f.Random.Int(2, 5)];
                                for (int i = 0; i < coords.Length; i++)
                                {
                                    coords[i] = new Coordinate(f.Address.Longitude(), f.Address.Latitude());
                                }
                                return geometryFactory.CreateLineString(coords);
                            case "Polygon":
                                var shellCoords = new Coordinate[4];
                                shellCoords[0] = new Coordinate(f.Address.Longitude(), f.Address.Latitude());
                                shellCoords[1] = new Coordinate(shellCoords[0].X + f.Random.Double(0.01, 0.1), shellCoords[0].Y);
                                shellCoords[2] = new Coordinate(shellCoords[1].X, shellCoords[0].Y + f.Random.Double(0.01, 0.1));
                                shellCoords[3] = new Coordinate(shellCoords[0].X, shellCoords[0].Y);
                                return geometryFactory.CreatePolygon(new LinearRing(shellCoords));
                            default:
                                return geometryFactory.CreatePoint(new Coordinate(f.Address.Longitude(), f.Address.Latitude()));
                        }
                    });

                var shapes = shapeFaker.Generate(count);

                foreach (var shape in shapes)
                {
                    await _unitOfWork.Points.AddAsync(shape);
                }
                
                await _unitOfWork.SaveAsync();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = $"{count} test shapes created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
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
        public Geometry? NewGeometry { get; set; }
    }
}