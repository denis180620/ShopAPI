using System.Net;

namespace ShopApi
{
    public class Result<T>
    {
        public int StatusCode {get; set;}
        public T data {get; set;}
        public string ErrorMessage {get; set;}
        public string? Message {get; set;}
        public bool IsSuccess {get; set;}
        public static Result<T> Success( T data, string Message)
        {
            return new Result<T> { IsSuccess = true, StatusCode = 200, data = data, Message = string.IsNullOrWhiteSpace(Message) ? null : Message };
        }
        public static new Result<T> Failure(int StatusCode, string ErrorMessage)
        {
            return new Result<T> {IsSuccess = false, StatusCode = StatusCode, ErrorMessage = ErrorMessage};
        }
    }
}