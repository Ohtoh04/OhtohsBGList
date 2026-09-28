using OhtohsBGList.Contracts.Publishers;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Mappings;

public static class PublisherMappings
{
    public static Publisher ToPublisher(this CreatePublisherRequest request)
    {
        return new Publisher
        {
            Name = request.Name
        };
    }

    public static CreatePublisherResponse ToCreatePublisherResponse(this Publisher publisher)
    {
        return new CreatePublisherResponse
        {
            Id = publisher.Id,
            Name = publisher.Name
        };
    }

    public static void ApplyUpdate(this Publisher publisher, UpdatePublisherRequest request)
    {
        publisher.Name = request.Name;
    }

    public static UpdatePublisherResponse ToUpdatePublisherResponse(this Publisher publisher)
    {
        return new UpdatePublisherResponse
        {
            Id = publisher.Id,
            Name = publisher.Name
        };
    }
}
