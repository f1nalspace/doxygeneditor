using DoxygenEditor.MVVM;
using DoxygenEditor.Parser.Entities;
using System;

namespace DoxygenEditor.ViewModels
{
    public class SymbolSearchResultItemViewModel : ViewModelBase
    {
        public string Id
        {
            get { return GetProperty(() => Id); }
            set { SetProperty(() => Id, value); }
        }
        public string Caption
        {
            get { return GetProperty(() => Caption); }
            set { SetProperty(() => Caption, value); }
        }
        public Type Type
        {
            get { return GetProperty(() => Type); }
            set { SetProperty(() => Type, value, () => RaisePropertyChanged(() => TypeString)); }
        }

        public Entity Entity { get; }
        public SymbolSearchResultItemViewModel(Entity entity)
        {
            Entity = entity;
        }

        public string TypeString
        {
            get { return Type.Name; }
        }
    }
}
