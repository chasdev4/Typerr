﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Typerr.Commands;
using Typerr.Model;
using Typerr.View;

namespace Typerr.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly User _user;

        private ObservableCollection<LibTileViewModel> _libTileViewModels;

        public ObservableCollection<LibTileViewModel> LibTileViewModels
        {
            get { return _libTileViewModels; }
            set { _libTileViewModels = value; }
        }

        private ObservableCollection<FeedTileViewModel> _feedTileViewModels;

        public ObservableCollection<FeedTileViewModel> FeedTileViewModels
        {
            get { return _feedTileViewModels; }
            set { _feedTileViewModels = value; }
        }

        private ObservableCollection<SubTileViewModel> _subTileViewModels;

        public ObservableCollection<SubTileViewModel> SubTileViewModels
        {
            get { return _subTileViewModels; }
            set { _subTileViewModels = value; }
        }

        public MainViewModel MainViewModel { get; }
        public NavPanelViewModel NavPanelViewModel { get; set; }
        public ICommand CreateTestTileCommand { get; }
        public ICommand AddSubscriptionTileCommand { get; }
        public ICommand GoToLibraryCommand { get; }

        private double _feedContentHeight;
        public double FeedContentHeight
        {
            get
            {
                return _feedContentHeight;
            }
            set
            {
                _feedContentHeight = value;
                OnPropertyChanged(nameof(FeedContentHeight));
            }
        }


        public HomeViewModel(MainViewModel mainViewModel, ICommand createTestTileCommand, ICommand goToLibraryCommand, User user)
        {
            MainViewModel = mainViewModel;
            CreateTestTileCommand = createTestTileCommand;
            GoToLibraryCommand = goToLibraryCommand;
            AddSubscriptionTileCommand = new AddSubscriptionTileCommand(mainViewModel);
            _libTileViewModels = new ObservableCollection<LibTileViewModel>();
            _feedTileViewModels = new ObservableCollection<FeedTileViewModel>();
            _subTileViewModels = new ObservableCollection<SubTileViewModel>();
            _user = user;
            Init();
            RefreshLibrary();
        }

        private void Init()
        {
            FeedContentHeight = 0;
        }

        public void RefreshLibrary()
        {
            IEnumerable<LibTileViewModel> result = MainViewModel.AllLibTileViewModels.Take(6);

            LibTileViewModels.Clear();

            foreach (var r in result)
            {
                LibTileViewModels.Add(r);
            }
        }
    }
}