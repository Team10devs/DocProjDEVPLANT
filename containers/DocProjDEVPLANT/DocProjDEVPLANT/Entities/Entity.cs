namespace DocProjDEVPLANT.Entities;

public class Entity : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}