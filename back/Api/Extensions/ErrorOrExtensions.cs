using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ErrorOrExtensions
{
    public static IActionResult ToActionResult<T>(this ErrorOr<T> errorOr)
    {
        if (!errorOr.IsError)
            return new OkObjectResult(errorOr.Value);
        
        var firstError = errorOr.FirstError;

        return firstError.Type switch
        {
            ErrorType.NotFound   => new NotFoundObjectResult(new { firstError.Code, firstError.Description }),
            ErrorType.Validation => new BadRequestObjectResult(new { firstError.Code, firstError.Description }),
            ErrorType.Conflict   => new ConflictObjectResult(new { firstError.Code, firstError.Description }),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(new { firstError.Code, firstError.Description }),
            ErrorType.Forbidden => new ObjectResult(new { firstError.Code, firstError.Description }) { StatusCode = StatusCodes.Status403Forbidden },
            
            _ => new BadRequestObjectResult(new { firstError.Code, firstError.Description })
        };
    }
}
