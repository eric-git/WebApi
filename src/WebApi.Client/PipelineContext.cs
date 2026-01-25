using WebApi.Client.Model;

namespace WebApi.Client;

internal sealed class PipelineContext
{
    public Guid CurrentGameId { get; set; }
    public Game? CurrentGame { get; set; }
    public Guid CurrentRelationId { get; set; }
    public Relation? CurrentRelation { get; set; }
}