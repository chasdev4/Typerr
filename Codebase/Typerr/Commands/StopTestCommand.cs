﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Typerr.Service;
using Typerr.Stores;
using Typerr.ViewModel;

namespace Typerr.Commands
{
    public class StopTestCommand : CommandBase
    {
        private readonly TestPanelViewModel _testPanelViewModel;
        private readonly MainViewModel _mainViewModel;

        public StopTestCommand(TestPanelViewModel testPanelViewModel, MainViewModel mainViewModel)
        {
            _testPanelViewModel = testPanelViewModel;
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            if (_testPanelViewModel.TestVM.UserText.Length < _testPanelViewModel.TestVM.Text.Length && _testPanelViewModel.TestVM.UserText.Length > 0)
            {
                _testPanelViewModel.TestVM.TestModel.testData.ErrorPositions = _testPanelViewModel.TestVM.ErrorPositions;
                _testPanelViewModel.TestVM.TestModel.testData.TestStarted = true;
                _testPanelViewModel.TestVM.TestModel.testData.LastPosition = _testPanelViewModel.TestVM.UserText.Length;
                TestService.Write(_testPanelViewModel.TestVM.TestModel);
            }    

            _mainViewModel.CurrentDialog = new ResultsViewModel(_testPanelViewModel, _testPanelViewModel.TestVM, _mainViewModel);
            _mainViewModel.OverlayVisibility = Visibility.Visible;
        }

        
    }
}
