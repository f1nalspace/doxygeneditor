namespace DoxygenEditor.Parsers.Entities
{
    public class CommentEntity : Entity
    {
        public override string Id => "";
        public override string DisplayName => "";

        public CommentEntity(SequenceInfo lineInfo) : base(lineInfo)
        {
        }
    }
}
