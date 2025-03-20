namespace Test_3TierAPI.Exceptions
{
    /// <summary>
    /// Endpoint에 대한 요청이 너무 많은 경우 발생하는 예외
    /// ExceptionHelper에서 429 상태코드로 변환하여 반환
    /// </summary>
    public class TooManyRequestsException : Exception
    {
        public TooManyRequestsException(string message) : base(message) { }
    }
}
