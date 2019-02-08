namespace TSP.DoxygenEditor.Parsers.Obsolete.Entities
{
    public class SectionEntity : Entity
    {
        public string SectionId { get; }
        public string SectionCaption { get; }
        public override string Id => SectionId;
        public override string DisplayName => SectionCaption;
        public SectionEntity(SequenceInfo lineInfo, string id, string caption) : base(lineInfo)
        {
            SectionId = id;
            SectionCaption = caption;
        }
    }
}
