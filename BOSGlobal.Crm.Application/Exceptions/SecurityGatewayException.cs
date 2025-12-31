namespace BOSGlobal.Crm.Application.Exceptions;

public class SecurityGatewayException : Exception
{
    public int StatusCode { get; }

    public SecurityGatewayException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
