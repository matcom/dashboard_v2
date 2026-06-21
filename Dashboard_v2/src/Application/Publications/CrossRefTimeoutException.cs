namespace Dashboard_v2.Application.Publications;

public sealed class CrossRefTimeoutException : Exception
{
    public CrossRefTimeoutException() : base("CrossRef did not respond in time.") { }
    public CrossRefTimeoutException(string message) : base(message) { }
    public CrossRefTimeoutException(string message, Exception inner) : base(message, inner) { }
}
