using Microsoft.AspNetCore.Mvc;
using WebApplication2.Entity;
using WebApplication2.Models;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PointController : ControllerBase
{
    private static List<Point> liste = new List<Point>();
    private static int idCounter = 1;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(ApiResponse<List<Point>>.SuccessResponse(liste, "Points retrieved successfully"));
    }
    
    [HttpGet]
    public IActionResult GetById(int id)
    {
        var point = liste.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return NotFound(ApiResponse<Point>.ErrorResponse($"Point with ID {id} not found.", "Point not found"));
        }
        return Ok(ApiResponse<Point>.SuccessResponse(point, "Point retrieved successfully"));
    }
    
    [HttpPost]
    public IActionResult UpdateById(int id, string newName, string newWKT)
    {
        var point = liste.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return NotFound(ApiResponse<Point>.ErrorResponse($"Point with ID {id} not found.", "Point not found"));
        }

        var errors = new List<string>();
        if (string.IsNullOrEmpty(newName) && string.IsNullOrEmpty(newWKT))
            errors.Add("At least one of Name or WKT must be provided");

        if (errors.Any())
        {
            return BadRequest(ApiResponse<Point>.ErrorResponse(errors, "Validation failed"));
        }

        if (!string.IsNullOrEmpty(newWKT) && !IsValidWKT(newWKT))
        {
            return BadRequest(ApiResponse<Point>.ErrorResponse("Invalid WKT format. Examples: POINT (30 10), LINESTRING (30 10, 10 30, 40 40), POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))", "Invalid WKT format"));
        }

        if (!string.IsNullOrEmpty(newName))
            point.Name = newName;
        
        if (!string.IsNullOrEmpty(newWKT))
            point.WKT = newWKT;

        return Ok(ApiResponse<Point>.SuccessResponse(point, "Point updated successfully"));
    }

    [HttpPost]
    public IActionResult DeleteById(int id)
    {
        var point = liste.FirstOrDefault(p => p.Id == id);
        if (point == null)
        {
            return NotFound(ApiResponse<Point>.ErrorResponse($"Point with ID {id} not found.", "Point not found"));
        }
        liste.Remove(point);
        
        
        return Ok(ApiResponse<Point>.SuccessResponse(point, "Point deleted successfully"));
    }
    

    [HttpPost]
    public IActionResult Add(string name, string WKT)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(name))
            errors.Add("Name cannot be empty");
        if (string.IsNullOrEmpty(WKT))
            errors.Add("WKT cannot be empty");
        
        if (errors.Any())
        {
            return BadRequest(ApiResponse<Point>.ErrorResponse(errors, "Validation failed"));
        }
        
        if (!IsValidWKT(WKT))
        {
            return BadRequest(ApiResponse<Point>.ErrorResponse("Invalid WKT format. Examples: POINT (30 10), LINESTRING (30 10, 10 30, 40 40), POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))", "Invalid WKT format"));
        }

        var point = new Point
        {
            Id = idCounter++,
            Name = name,
            WKT = WKT
        };
        
        liste.Add(point);
        return Ok(ApiResponse<Point>.SuccessResponse(point, "Point added successfully"));
    }
    
    [HttpPost]
    public IActionResult AddRange(List<Point> points)
    {
        if (points == null || points.Count == 0)
        {
            return BadRequest(ApiResponse<List<Point>>.ErrorResponse("The list of points cannot be empty.", "Invalid input"));
        }

        var errors = new List<string>();
        var validPoints = new List<Point>();

        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var pointErrors = new List<string>();
            
            if (string.IsNullOrEmpty(point.Name))
                pointErrors.Add($"Point {i + 1}: Name cannot be empty");
            if (string.IsNullOrEmpty(point.WKT))
                pointErrors.Add($"Point {i + 1}: WKT cannot be empty");
            else if (!IsValidWKT(point.WKT))
                pointErrors.Add($"Point {i + 1} ('{point.Name}'): Invalid WKT format");
            
            if (pointErrors.Any())
            {
                errors.AddRange(pointErrors);
            }
            else
            {
                validPoints.Add(point);
            }
        }

        if (errors.Any())
        {
            return BadRequest(ApiResponse<List<Point>>.ErrorResponse(errors, "Validation failed for one or more points"));
        }
        foreach (var point in validPoints)
        {
            point.Id = idCounter++;
        }

        liste.AddRange(validPoints);

        return Ok(ApiResponse<List<Point>>.SuccessResponse(validPoints, $"Successfully added {validPoints.Count} points"));
    }
    
    private bool IsValidWKT(string wkt)
    {
        if (string.IsNullOrEmpty(wkt))
            return false;

        // Basic format validation for common WKT types
        if (wkt.StartsWith("POINT", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.RegularExpressions.Regex.IsMatch(wkt, 
                @"^POINT\s*\(\s*-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*\)$", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        else if (wkt.StartsWith("LINESTRING", StringComparison.OrdinalIgnoreCase))
        {
            return wkt.Contains("(") && wkt.Contains(")") && 
                   System.Text.RegularExpressions.Regex.IsMatch(wkt, @"\d+(\.\d+)?\s+\d+(\.\d+)?,");
        }
        else if (wkt.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
        {
            return wkt.Contains("((") && wkt.Contains("))") && 
                   System.Text.RegularExpressions.Regex.IsMatch(wkt, @"\d+(\.\d+)?\s+\d+(\.\d+)?,");
        }
    
        return false;
    }
}