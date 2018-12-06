namespace DoxygenEditor.MVVM
{
    public abstract class ViewModelBase : BindableBase
    {
        public virtual void ViewLoaded(object view) { }
        protected T GetService<T>() where T : class
        {
            var r = IOCContainer.Default.Get<T>();
            return (r);
        }
    }
}
