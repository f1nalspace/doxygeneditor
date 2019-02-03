using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoxygenEditor.Parsers.Entities
{
    public class ParamEntity : Entity
    {
        public override string Id => "";
        public override string DisplayName => ParamName;

        public string ParamName { get; }
        public string ParamValue { get; }

        public ParamEntity(SequenceInfo lineInfo, string paramName, string paramValue) : base(lineInfo)
        {
            ParamName = paramName;
            ParamValue = paramValue;
        }
    }
}
