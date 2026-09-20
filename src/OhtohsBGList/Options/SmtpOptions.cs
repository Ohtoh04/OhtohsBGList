namespace OhtohsBGList.Options;

public class SmtpOptions
{
    public required string Host { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public required string FromAddress { get; init; }
    public required string FromName { get; init; }
}
