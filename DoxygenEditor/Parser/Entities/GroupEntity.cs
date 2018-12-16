namespace DoxygenEditor.Parser.Entities
{
    public class GroupEntity : Entity
    {
        public string GroupId { get; }
        public string GroupCaption { get; }
        public override string DisplayName => GroupCaption;

        public GroupEntity(SequenceInfo lineInfo, string id, string caption) : base(lineInfo)
        {
            GroupId = id;
            GroupCaption = caption;
        }
    }
}
