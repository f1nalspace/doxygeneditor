using System;
using System.Collections.Generic;

namespace DoxygenEditor.MVVM
{
    public class PropertyManager
    {
        internal Dictionary<string, object> _propertiesMap = new Dictionary<string, object>();
        public bool SetProperty<T>(string propertyName, T value, Action<string> raiseNotification, Action changedCallback)
        {
            T oldValue;
            var res = SetPropertyInternal(propertyName, value, out oldValue);
            if (res)
            {
                raiseNotification?.Invoke(propertyName);
                changedCallback?.Invoke();
            }
            return res;
        }
        public bool SetProperty<T>(string propertyName, T value, Action<string> raiseNotification, Action<T> changedCallback)
        {
            T oldValue;
            var res = SetPropertyInternal(propertyName, value, out oldValue);
            if (res)
            {
                raiseNotification?.Invoke(propertyName);
                changedCallback?.Invoke(oldValue);
            }
            return res;
        }
        protected virtual bool SetPropertyInternal<T>(string propertyName, T value, out T oldValue)
        {
            oldValue = default(T);
            object val;
            if (_propertiesMap.TryGetValue(propertyName, out val))
                oldValue = (T)val;
            if (CompareValues<T>(oldValue, value))
                return false;
            _propertiesMap[propertyName] = value;
            return true;
        }

        public T GetProperty<T>(string propertyName)
        {
            object val;
            if (_propertiesMap.TryGetValue(propertyName, out val))
                return (T)val;
            return default(T);
        }
        public bool SetProperty<T>(ref T storage, T value, string propertyName, Action<string> raiseNotification, Action changedCallback)
        {
            T oldValue = storage;
            var res = SetPropertyCore(ref storage, value, propertyName);
            if (res)
            {
                raiseNotification?.Invoke(propertyName);
                changedCallback?.Invoke();
            }
            return res;
        }
        public bool SetProperty<T>(ref T storage, T value, string propertyName, Action<string> raiseNotification, Action<T> changedCallback)
        {
            T oldValue = storage;
            var res = SetPropertyCore(ref storage, value, propertyName);
            if (res)
            {
                raiseNotification?.Invoke(propertyName);
                changedCallback?.Invoke(oldValue);
            }
            return res;
        }
        protected virtual bool SetPropertyCore<T>(ref T storage, T value, string propertyName)
        {
            if (CompareValues<T>(storage, value))
                return false;
            storage = value;
            return true;
        }

        static bool CompareValues<T>(T storage, T value)
        {
            return object.Equals(storage, value);
        }
    }
}
