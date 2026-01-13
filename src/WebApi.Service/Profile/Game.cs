namespace WebApi.Service.Profile;

public class Game : AutoMapper.Profile
{
    public Game()
    {
        CreateMap<DataAccess.Entity.Game, Model.Game>()
            .ReverseMap();
    }
}