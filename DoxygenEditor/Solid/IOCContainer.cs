using System;
using System.Collections.Generic;

namespace DoxygenEditor.Solid
{
    public static class IOCContainer
    {
        private static readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public static void Register(object instance)
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

        public static T Get<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (_instances.ContainsKey(interfaceType))
                return (T)_instances[interfaceType];
            return (null);
        }
    }
}
