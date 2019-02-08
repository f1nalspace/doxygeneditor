namespace TSP.DoxygenEditor.Parsers.Obsolete.Entities
{
    public class DeclarationEntity : Entity
    {
        public override string Id => Ident;
        public override string DisplayName => Ident;
        public string Ident { get; }

        public enum DeclType
        {
            None = 0,
            Function,
            Typedef,
            Define
        }
        public DeclType DeclarationType { get; }

        public DeclarationEntity(SequenceInfo lineInfo, string ident, DeclType declType) : base(lineInfo)
        {
            Ident = ident;
            DeclarationType = declType;
        }
    }
}
