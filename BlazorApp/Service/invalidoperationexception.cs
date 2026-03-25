
namespace BlazorApp.Service
{
    [Serializable]
    internal class invalidoperationexception : Exception
    {
        public invalidoperationexception()
        {
        }

        public invalidoperationexception(string? message) : base(message)
        {
        }

        public invalidoperationexception(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}