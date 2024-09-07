namespace backend.Exceptions
{
    public class RenderingException : Exception
    {
        public RenderingException()
        {
        }

        public RenderingException(string? message) : base(message)
        {
        }

        public RenderingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
