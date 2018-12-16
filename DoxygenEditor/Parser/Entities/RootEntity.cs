using System;

namespace DoxygenEditor.Parser.Entities
{
    public class RootEntity : Entity
    {
        public override string DisplayName => string.Empty;
        public RootEntity() : base(new SequenceInfo())
        {

        }
    }
}
