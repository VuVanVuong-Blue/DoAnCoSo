namespace System_Music.Models.Common
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResult<T> Success(T data, string message = "Thao tác thành công")
        {
            return new ApiResult<T> { IsSuccess = true, Data = data, Message = message };
        }

        public static ApiResult<T> Failure(string message, List<string> errors = null)
        {
            return new ApiResult<T> { IsSuccess = false, Message = message, Errors = errors ?? new List<string>() };
        }

        public static ApiResult<T> Failure(string message, string error)
        {
            return new ApiResult<T> { IsSuccess = false, Message = message, Errors = new List<string> { error } };
        }
    }
}
