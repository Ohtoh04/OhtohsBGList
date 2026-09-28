using OhtohsBGList.Contracts.Mechanics;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Mappings;

public static class MechanicMappings
{
    public static Mechanic ToMechanic(this CreateMechanicRequest request)
    {
        return new Mechanic
        {
            Name = request.Name,
            Notes = request.Notes,
            Flags = request.Flags
        };
    }

    public static CreateMechanicResponse ToCreateMechanicResponse(this Mechanic mechanic)
    {
        return new CreateMechanicResponse
        {
            Id = mechanic.Id,
            Name = mechanic.Name,
            Notes = mechanic.Notes,
            Flags = mechanic.Flags
        };
    }

    public static void ApplyUpdate(this Mechanic mechanic, UpdateMechanicRequest request)
    {
        mechanic.Name = request.Name;
        mechanic.Notes = request.Notes;
        mechanic.Flags = request.Flags;
    }

    public static UpdateMechanicResponse ToUpdateMechanicResponse(this Mechanic mechanic)
    {
        return new UpdateMechanicResponse
        {
            Id = mechanic.Id,
            Name = mechanic.Name,
            Notes = mechanic.Notes,
            Flags = mechanic.Flags
        };
    }
}
