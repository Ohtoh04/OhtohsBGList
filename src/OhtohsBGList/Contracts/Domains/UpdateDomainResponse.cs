namespace OhtohsBGList.Contracts.Domains;

public class UpdateDomainResponse
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public string? Notes { get; set; }

    public required int Flags { get; set; }
}
