using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Microsoft.AspNetCore.Mvc;

namespace BaşarsoftStaj.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PointController : ControllerBase
{
    private readonly IPointService pointService;

    public PointController(IPointService pointService)
    {
        this.pointService = pointService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = pointService.GetAllPoints();
        return Ok(result);
    }
    
    [HttpGet]
    public IActionResult GetById(int id)
    {
        var result = pointService.GetPointById(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
    
    [HttpPost]
    public IActionResult UpdateById(int id, string newName, string newWKT)
    {
        var result = pointService.UpdatePoint(id, newName, newWKT);
        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public IActionResult DeleteById(int id)
    {
        var result = pointService.DeletePoint(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public IActionResult Add([FromBody] AddPointDto pointDto)
    {
        var result = pointService.AddPoint(pointDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
    
    [HttpPost]
    public IActionResult AddRange([FromBody] List<AddPointDto> pointDtos)
    {
        var result = pointService.AddRangePoints(pointDtos);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}