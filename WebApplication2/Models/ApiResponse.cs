namespace WebApplication2.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }
    
    public static ApiResponse<T> SuccessResponse(string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = default(T),
            Errors = new List<string>()
        };
    }
    
    public static ApiResponse<T> ErrorResponse(string error, string message = "Operation failed")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default(T),
            Errors = new List<string> { error }
        };
    }
    
    public static ApiResponse<T> ErrorResponse(List<string> errors, string message = "Operation failed")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default(T),
            Errors = errors
        };
    }
}


//Data olmayanlar için
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse(string message = "Operation completed successfully")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Data = null,
            Errors = new List<string>()
        };
    }

    public static ApiResponse ErrorResponse(string error, string message = "Operation failed")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = new List<string> { error }
        };
    }

    public static ApiResponse ErrorResponse(List<string> errors, string message = "Operation failed")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors
        };
    }
}
