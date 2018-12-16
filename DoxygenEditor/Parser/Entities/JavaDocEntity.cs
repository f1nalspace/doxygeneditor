using System.Collections.Generic;

namespace DoxygenEditor.Parser.Entities
{
    public class JavaDocEntity : Entity
    {
        public string Brief { get; set; }
        public string Details { get; set; }

        public override string DisplayName => string.Empty;

        public JavaDocEntity(SequenceInfo lineInfo) : base(lineInfo)
        {

        }
    }
}
