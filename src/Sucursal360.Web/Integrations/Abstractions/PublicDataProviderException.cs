namespace Sucursal360.Web.Integrations.Abstractions;

public sealed class PublicDataProviderException(
    string errorCode,
    string userMessage,
    string technicalMessage,
    Exception? innerException = null) : Exception(technicalMessage, innerException)
{
    public string ErrorCode { get; } = errorCode;

    public string UserMessage { get; } = userMessage;

    public string TechnicalMessage { get; } = technicalMessage;
}
