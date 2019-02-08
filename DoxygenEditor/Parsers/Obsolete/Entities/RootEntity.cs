using System;

namespace TSP.DoxygenEditor.Parsers.Obsolete.Entities
{
    public class RootEntity : Entity
    {
        public override string Id => string.Empty;
        public override string DisplayName => string.Empty;
        public RootEntity() : base(new SequenceInfo())
        {

        }
    }
}
