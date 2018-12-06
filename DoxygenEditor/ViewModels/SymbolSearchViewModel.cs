using DoxygenEditor.MVVM;
using DoxygenEditor.Parser;
using DoxygenEditor.Parser.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DoxygenEditor.ViewModels
{
    public class SymbolSearchViewModel : ViewModelBase
    {
        public delegate void UpdatedSearchResultsEventHandler(object sender, IEnumerable<SymbolSearchResultItemViewModel> results);
        public event UpdatedSearchResultsEventHandler UpdatedSearchResults;

        public string SearchText
        {
            get { return GetProperty(() => SearchText); }
            set { SetProperty(() => SearchText, value); }
        }
        public Type SearchType
        {
            get { return GetProperty(() => SearchType); }
            set { SetProperty(() => SearchType, value); }
        }
        public SymbolSearchResultItemViewModel SelectedResultItem
        {
            get { return GetProperty(() => SelectedResultItem); }
            set { SetProperty(() => SelectedResultItem, value, () => JumpToCommand.RaiseCanExecuteChanged()); }
        }

        public Type[] SearchTypes
        {
            get
            {
                return new[] {
                    typeof(PageEntity),
                    typeof(SectionEntity),
                    typeof(SubSectionEntity),
                };
            }
        }

        public ICommand SearchCommand { get; }
        public DelegateCommand<SymbolSearchResultItemViewModel> JumpToCommand { get; }

        private readonly List<SymbolSearchResultItemViewModel> _searchResults;
        private readonly MainViewModel _mainViewModel;

        private bool IsMatch(PageEntity page, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;
            Regex rex = new Regex(Regex.Escape(searchText), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (page.PageId != null && rex.IsMatch(page.PageId))
                return true;
            if (page.PageCaption != null && rex.IsMatch(page.PageCaption))
                return true;
            return false;
        }
        private bool IsMatch(SectionEntity section, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;
            Regex rex = new Regex(Regex.Escape(searchText), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (section.SectionId != null && rex.IsMatch(section.SectionId))
                return true;
            if (section.SectionCaption != null && rex.IsMatch(section.SectionCaption))
                return true;
            return false;
        }

        private void UpdateSearchResultsForEntity(Entity entity, string searchText)
        {
            if (!entity.GetType().Equals(typeof(RootEntity)))
            {
                Type t = entity.GetType();
                bool allowed = false;
                foreach (var searchType in SearchTypes)
                {
                    if (searchType.IsInstanceOfType(entity))
                    {
                        allowed = true;
                        break;
                    }
                }
                if (allowed)
                {
                    if (SearchType != null)
                        allowed = SearchType.IsInstanceOfType(entity);
                    else
                        allowed = true;
                }
                if (allowed)
                {
                    SymbolSearchResultItemViewModel item = new SymbolSearchResultItemViewModel(entity);
                    item.Type = t;
                    if (typeof(PageEntity).IsInstanceOfType(entity))
                    {
                        PageEntity page = (PageEntity)entity;
                        if (IsMatch(page, searchText))
                        {
                            item.Id = page.PageId;
                            item.Caption = page.PageCaption;
                            _searchResults.Add(item);
                        }
                    }
                    else if (typeof(SectionEntity).IsInstanceOfType(entity))
                    {
                        SectionEntity section = (SectionEntity)entity;
                        if (IsMatch(section, searchText))
                        {
                            item.Id = section.SectionId;
                            item.Caption = section.SectionCaption;
                            _searchResults.Add(item);
                        }
                    }
                    else throw new Exception($"Invalid type '{t.FullName}'!");
                }
            }
            foreach (var child in entity.Children)
                UpdateSearchResultsForEntity(child, searchText);
        }

        public SymbolSearchViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            SearchCommand = new DelegateCommand<object>((e) =>
            {
                ParseState parseState = _mainViewModel.ParseState;
                _searchResults.Clear();
                UpdateSearchResultsForEntity(parseState.RootEntity, SearchText);
                UpdatedSearchResults?.Invoke(this, _searchResults);
            });
            JumpToCommand = new DelegateCommand<SymbolSearchResultItemViewModel>((e) =>
            {
                Entity entity = (Entity)e.Entity;
                int pos = entity.LineInfo.Start;
                _mainViewModel.GoToPositionCommand.Execute(pos);
            }, (e) => { return SelectedResultItem != null; } );

            _searchResults = new List<SymbolSearchResultItemViewModel>();
        }
    }
}
