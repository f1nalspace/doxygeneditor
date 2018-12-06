using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DoxygenEditor.MVVM
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private PropertyManager _propertyManager;
        internal PropertyManager PropertyManager { get { return _propertyManager ?? (_propertyManager = CreatePropertyManager()); } }
        private PropertyManager CreatePropertyManager()
        {
            return new PropertyManager();
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetPropertyName(LambdaExpression lambda)
        {
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }
            var propertyInfo = memberExpression.Member as PropertyInfo;
            return propertyInfo.Name;
        }
        protected void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            string propName = GetPropertyName(expression);
            RaisePropertyChanged(propName);
        }
        protected void RaisePropertiesChanged(params string[] properties)
        {
            foreach (var property in properties)
                RaisePropertyChanged(property);
        }
        protected void RaisePropertiesChanged<T1, T2>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2)
        {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
        }
        protected void RaisePropertiesChanged<T1, T2, T3>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3)
        {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
            RaisePropertyChanged(expression3);
        }
        protected void RaisePropertiesChanged<T1, T2, T3, T4>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3, Expression<Func<T4>> expression4)
        {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
            RaisePropertyChanged(expression3);
            RaisePropertyChanged(expression4);
        }
        protected void RaisePropertiesChanged<T1, T2, T3, T4, T5>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3, Expression<Func<T4>> expression4, Expression<Func<T5>> expression5)
        {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
            RaisePropertyChanged(expression3);
            RaisePropertyChanged(expression4);
            RaisePropertyChanged(expression5);
        }
        protected T GetProperty<T>(Expression<Func<T>> expression)
        {
            return PropertyManager.GetProperty<T>(GetPropertyName(expression));
        }
        protected bool SetProperty<T>(ref T storage, T value, string propertyName, Action changedCallback)
        {
            return PropertyManager.SetProperty<T>(ref storage, value, propertyName, RaisePropertyChanged, changedCallback);
        }
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> expression, Action changedCallback)
        {
            return SetProperty(ref storage, value, GetPropertyName(expression), changedCallback);
        }
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> expression)
        {
            return SetProperty(ref storage, value, expression, null);
        }
        protected bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            return SetProperty<T>(ref storage, value, propertyName, null);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value)
        {
            return SetProperty(expression, value, (Action)null);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action changedCallback)
        {
            string propertyName = GetPropertyName(expression);
            return PropertyManager.SetProperty<T>(propertyName, value, RaisePropertyChanged, changedCallback);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action<T> changedCallback)
        {
            string propertyName = GetPropertyName(expression);
            return PropertyManager.SetProperty<T>(propertyName, value, RaisePropertyChanged, changedCallback);
        }
    }
}
