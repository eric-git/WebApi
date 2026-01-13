namespace WebApi.Service.Profile;

public class Relation : AutoMapper.Profile
{
    public Relation()
    {
        CreateMap<DataAccess.Entity.Relation, Model.Relation>()
            .ForMember(d => d.Attributes, opt => opt.MapFrom(s => s.AttributesMap))
            .ReverseMap();
    }
}