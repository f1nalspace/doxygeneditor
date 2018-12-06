using DoxygenEditor.Controls;
using DoxygenEditor.MVVM;
using System.Collections.Generic;

namespace DoxygenEditor.ViewModels
{
    public class SearchReplaceViewModel : ViewModelBase
    {
        private readonly ISearchReplaceControl _control;

        public string SearchText
        {
            get { return GetProperty(() => SearchText); }
            set { SetProperty(() => SearchText, value); }
        }
        public string ReplaceText
        {
            get { return GetProperty(() => ReplaceText); }
            set { SetProperty(() => ReplaceText, value); }
        }
        public bool MatchCase
        {
            get { return GetProperty(() => MatchCase); }
            set { SetProperty(() => MatchCase, value); }
        }
        public bool WholeWord
        {
            get { return GetProperty(() => WholeWord); }
            set { SetProperty(() => WholeWord, value); }
        }
        public bool Wrap
        {
            get { return GetProperty(() => Wrap); }
            set { SetProperty(() => Wrap, value); }
        }
        public bool IsRegex
        {
            get { return GetProperty(() => IsRegex); }
            set { SetProperty(() => IsRegex, value); }
        }
        public bool IsShown
        {
            get { return _control.IsShown(); }
        }

        private readonly List<string> _searchHistory;
        public IEnumerable<string> SearchHistory
        {
            get { return _searchHistory; }
        }

        private readonly List<string> _replaceHistory;
        public IEnumerable<string> ReplaceHistory
        {
            get { return _replaceHistory; }
        }

        public enum SearchDirection
        {
            Prev,
            Next,
        }
        public enum ReplaceMode
        {
            Next,
            All,
        }

        public delegate void SearchExecutedEventHandler(SearchReplaceViewModel vm, SearchDirection direction);
        public delegate void ReplaceExecutedEventHandler(SearchReplaceViewModel vm, ReplaceMode mode);
        public event SearchExecutedEventHandler SearchExecuted;
        public event ReplaceExecutedEventHandler ReplaceExecuted;

        public DelegateCommand<object> ShowSearchCommand { get; }
        public DelegateCommand<object> ShowReplaceCommand { get; }
        public DelegateCommand<object> HideCommand { get; }
        public DelegateCommand<SearchDirection> SearchExecutedCommand { get; }
        public DelegateCommand<ReplaceMode> ReplaceExecutedCommand { get; }

        public SearchReplaceViewModel(ISearchReplaceControl control)
        {
            _control = control;
            _searchHistory = new List<string>();
            _replaceHistory = new List<string>();

            ShowSearchCommand = new DelegateCommand<object>((e) => {
                _control.ShowSearchOnly(true);
            });
            ShowReplaceCommand = new DelegateCommand<object>((e) => {
                _control.ShowSearchAndReplace(true);
            });
            HideCommand = new DelegateCommand<object>((e) => {
                _control.HideSearchReplace();
            });

            SearchExecutedCommand = new DelegateCommand<SearchDirection>((e) => {
                string searchText = SearchText;
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!_searchHistory.Contains(searchText))
                        _searchHistory.Add(searchText);
                    SearchExecuted?.Invoke(this, e);
                }
            });
            ReplaceExecutedCommand = new DelegateCommand<ReplaceMode>((e) => {
                string replaceText = ReplaceText;
                if (replaceText != null)
                {
                    if (!_replaceHistory.Contains(replaceText))
                        _replaceHistory.Add(replaceText);
                    ReplaceExecuted?.Invoke(this, e);
                }
            });
        }


    }
}
