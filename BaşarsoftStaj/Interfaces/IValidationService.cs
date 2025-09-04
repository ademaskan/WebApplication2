using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaşarsoftStaj.Interfaces
{
    public interface IValidationService
    {
        Task<ApiResponse<object>> ValidateShapeAsync(Shape shape, List<Shape>? pendingShapes = null);
    }
}
