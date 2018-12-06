namespace DoxygenEditor.MVVM
{
    public abstract class IOCContainer
    {
        private static IOCContainer _container;
        public static IOCContainer Default { get { return _container ?? (_container = new DefaultIOCContainer()); } }
        public abstract void Register(object instance);
        public abstract T Get<T>() where T : class;
    }
}
