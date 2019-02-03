using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoxygenEditor.Parsers.Entities
{
    public class SubSectionEntity : SectionEntity
    {
        public SubSectionEntity(SequenceInfo lineInfo, string id, string caption) : base(lineInfo, id, caption)
        {
        }
    }
}
