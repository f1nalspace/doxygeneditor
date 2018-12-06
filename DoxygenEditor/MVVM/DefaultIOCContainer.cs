using System;
using System.Collections.Generic;

namespace DoxygenEditor.MVVM
{
    class DefaultIOCContainer : IOCContainer
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public override void Register(object instance)
        {
            Type classType = instance.GetType();
            Type[] interfaceTypes = classType.GetInterfaces();
            foreach (Type interfaceType in interfaceTypes)
            {
                if (!_instances.ContainsKey(interfaceType))
                    _instances.Add(interfaceType, instance);
                else
                    _instances[interfaceType] = instance;
            }
        }

        public override T Get<T>()
        {
            Type interfaceType = typeof(T);
            if (_instances.ContainsKey(interfaceType))
                return (T)_instances[interfaceType];
            return null;
        }
    }
}
