using System;

namespace DoxygenEditor.Models
{
    public class TypeStringModel
    {
        private readonly Type _type;
        public Type Type { get { return _type; } }

        public TypeStringModel(Type type)
        {
            _type = type;
        }
        public override string ToString()
        {
            if (_type == null)
                return "All types";
            else
                return _type.Name;
        }
    }
}
