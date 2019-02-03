using System.Collections.Generic;

namespace DoxygenEditor.Parsers.Entities
{
    public class JavaDocEntity : Entity
    {
        public string Brief { get; set; }
        public string Details { get; set; }
        public override string Id => string.Empty;
        public override string DisplayName => string.Empty;

        public JavaDocEntity(SequenceInfo lineInfo) : base(lineInfo)
        {

        }
    }
}
