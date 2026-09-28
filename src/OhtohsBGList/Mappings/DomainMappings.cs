using OhtohsBGList.Contracts.Domains;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Mappings;

public static class DomainMappings
{
    public static Domain ToDomain(this CreateDomainRequest request)
    {
        return new Domain
        {
            Name = request.Name,
            Notes = request.Notes,
            Flags = request.Flags
        };
    }

    public static CreateDomainResponse ToCreateDomainResponse(this Domain domain)
    {
        return new CreateDomainResponse
        {
            Id = domain.Id,
            Name = domain.Name,
            Notes = domain.Notes,
            Flags = domain.Flags
        };
    }

    public static void ApplyUpdate(this Domain domain, UpdateDomainRequest request)
    {
        domain.Name = request.Name;
        domain.Notes = request.Notes;
        domain.Flags = request.Flags;
    }

    public static UpdateDomainResponse ToUpdateDomainResponse(this Domain domain)
    {
        return new UpdateDomainResponse
        {
            Id = domain.Id,
            Name = domain.Name,
            Notes = domain.Notes,
            Flags = domain.Flags
        };
    }
}
