using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using BaşarsoftStaj.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaşarsoftStaj.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PointControllerStatic : ControllerBase
{
    private readonly PointServiceStatic pointService;

    public PointControllerStatic(PointServiceStatic pointService)
    {
        this.pointService = pointService;
    }

    [HttpGet]
    public ApiResponse<List<PointE>> GetAll()
    {
        return pointService.GetAllPoints();
    }
    
    [HttpGet]
    public ApiResponse<PointE> GetById(int id)
    {
        return pointService.GetPointById(id);
    }
    
    [HttpPost]
    public ApiResponse<PointE> UpdateById(int id, string newName, string newWKT)
    {
        return pointService.UpdatePoint(id, newName, newWKT);
    }

    [HttpPost]
    public ApiResponse<PointE> DeleteById(int id)
    {
        return pointService.DeletePoint(id);
    }

    [HttpPost]
    public ApiResponse<PointE> Add([FromBody] AddPointDto pointDto)
    {
        return pointService.AddPoint(pointDto);
    }
    
    [HttpPost]
    public ApiResponse<List<PointE>> AddRange([FromBody] List<AddPointDto> pointDtos)
    {
        return pointService.AddRangePoints(pointDtos);
    }
}