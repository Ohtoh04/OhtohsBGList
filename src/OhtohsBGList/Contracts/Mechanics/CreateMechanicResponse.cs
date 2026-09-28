namespace OhtohsBGList.Contracts.Mechanics;

public class CreateMechanicResponse
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public string? Notes { get; set; }

    public required int Flags { get; set; }
}
