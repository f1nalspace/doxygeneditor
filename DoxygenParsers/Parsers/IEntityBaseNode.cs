namespace TSP.DoxygenEditor.Parsers
{
    public interface IEntityBaseNode<TEntity> : IBaseNode where TEntity : BaseEntity
    {
        TEntity Entity { get; }
    }
}
