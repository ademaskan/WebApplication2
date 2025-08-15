namespace BaşarsoftStaj.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    
    public static ApiResponse<T> SuccessResponse(T data, string messageKey = "OperationCompletedSuccessfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = ApiMessageManager.GetMessage(messageKey),
            Data = data
        };
    }
    
    public static ApiResponse<T> SuccessResponse(string messageKey = "OperationCompletedSuccessfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = ApiMessageManager.GetMessage(messageKey),
            Data = default(T)
        };
    }
    
    public static ApiResponse<T> ErrorResponse(string messageKey = "OperationFailed")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = ApiMessageManager.GetMessage(messageKey),
            Data = default(T)
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static new ApiResponse SuccessResponse(string messageKey = "OperationCompletedSuccessfully")
    {
        return new ApiResponse
        {
            Success = true,
            Message = ApiMessageManager.GetMessage(messageKey),
            Data = null
        };
    }

    public static new ApiResponse ErrorResponse(string messageKey = "OperationFailed")
    {
        return new ApiResponse
        {
            Success = false,
            Message = ApiMessageManager.GetMessage(messageKey),
            Data = null
        };
    }
}
