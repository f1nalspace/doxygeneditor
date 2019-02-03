namespace DoxygenEditor.Parsers.Entities
{
    public class GroupEntity : Entity
    {
        public string GroupId { get; }
        public string GroupCaption { get; }
        public override string Id => GroupId;
        public override string DisplayName => GroupCaption;

        public GroupEntity(SequenceInfo lineInfo, string id, string caption) : base(lineInfo)
        {
            GroupId = id;
            GroupCaption = caption;
        }
    }
}
