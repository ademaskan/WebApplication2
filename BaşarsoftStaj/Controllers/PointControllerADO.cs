using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using BaşarsoftStaj.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaşarsoftStaj.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PointControllerADO : ControllerBase
{
    private readonly PointServiceADO pointService;

    public PointControllerADO(PointServiceADO pointService)
    {
        this.pointService = pointService;
    }

    [HttpGet]
    public ApiResponse<List<Point>> GetAll()
    {
        return pointService.GetAllPoints();
    }

    [HttpGet]
    public ApiResponse<Point> GetById(int id)
    {
        return pointService.GetPointById(id);
    }

    [HttpPost]
    public ApiResponse<Point> UpdateById(int id, string newName, string newWKT)
    {
        return pointService.UpdatePoint(id, newName, newWKT);
    }

    [HttpPost]
    public ApiResponse<Point> DeleteById(int id)
    {
        return pointService.DeletePoint(id);
    }

    [HttpPost]
    public ApiResponse<Point> Add([FromBody] AddPointDto pointDto)
    {
        return pointService.AddPoint(pointDto);
    }

    [HttpPost]
    public ApiResponse<List<Point>> AddRange([FromBody] List<AddPointDto> pointDtos)
    {
        return pointService.AddRangePoints(pointDtos);
    }
}