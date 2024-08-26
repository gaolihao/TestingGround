
using ProtoBuf;

namespace MyApi.Contract;


[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class FeatureList
{
    public FeatureList() { }

    public FeatureList(Guid Id) 
    { 
        this.Id = Id;
    }

    public Guid Id { get; set; }

    public MessageType MessageType { get; init; }
}
