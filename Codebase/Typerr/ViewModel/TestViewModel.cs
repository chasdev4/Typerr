﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Typerr.Commands;
using Typerr.Components;
using Typerr.Model;

namespace Typerr.ViewModel
{
    public class TestViewModel : ViewModelBase
    {
        public TestModel TestModel { get; }

        private readonly User _user;

        public TestPanelViewModel TestPanelVM { get; set; }
        // Everything the user has typed so far
        private string _userText;
        public string UserText
        {
            get => _userText;
            set
            {
                if (!_isPaused)
                {
                    _previousUserText = _userText;
                    _userText = value;
                    OnPropertyChanged(nameof(UserText));
                    if (_testStarted)
                    {
                        UpdateTest();
                    }
                }

            }
        }

        public int CorrectWords { get; private set; }

        private string _previousUserText = "";

        private bool _testStarted = false;
        private bool _isPaused = false;

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private RichTextBox _richTextBlock;
        public RichTextBox RichTextBlock
        {
            get => _richTextBlock;
            set
            {
                _richTextBlock = value;
                OnPropertyChanged(nameof(RichTextBlock));
            }
        }

        // List of runs to add to the RichTextBlock
        private List<Run> _runs;

        // The list of error positions is needed to know when to undo typing errors
        private List<int> _errorPositions;

        // These variables are related to tracking if the user got a word wrong
        private string[] _words;
        private bool[] _correctWordMap;

        public TestViewModel(TestModel testModel, User user)
        {
            TestModel = testModel;
            _user = user;
            Init();
        }

        private void Init()
        {
            _errorPositions = new List<int>();
            _words = TestModel.article.text.Split(" ");
            _correctWordMap = new bool[_words.Length];
            _correctWordMap[0] = true;
            _runs = new List<Run>();
            Text = TestModel.article.text;
            RichTextBlock = new RichTextBox();
            RichTextBlock.IsReadOnly = true;
            RichTextBlock.IsHitTestVisible = false;
            RichTextBlock.BorderThickness = new Thickness(0);
            UserText = "";
            _runs.Add(BuildRun(Text[0].ToString(), RunType.Current));
            _runs.Add(BuildRun(Text[1..], RunType.Untyped));
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(_runs[0]);
            paragraph.Inlines.Add(_runs[1]);
            RichTextBlock.Document.Blocks.Add(paragraph);
            _testStarted = true;
        }

        enum RunType
        {
            Current,
            Right,
            Wrong,
            Untyped
        }

        private Run BuildRun(string text, RunType runType)
        {
            return RunBuilderTask(text, runType);
        }

        private Run BuildRun(char text, RunType runType)
        {
            return RunBuilderTask(text.ToString(), runType);
        }

        private Run RunBuilderTask(string doNotUse, RunType runAway)
        {
            
            Run run = new Run(doNotUse);
            run.FontSize = 45;
            run.Foreground = new SolidColorBrush(
                (runAway == RunType.Right)
                ? Colors.Black
                : (runAway == RunType.Wrong)
                ? Colors.Red
                : Colors.LightGray);
            run.Background = new SolidColorBrush(
                (runAway == RunType.Current)
                ? Colors.DarkGray
                : (doNotUse == " " && runAway == RunType.Wrong)
                ? Color.FromArgb(64, 255, 0, 0)
                : Colors.White);

            return run;
        }

        private void UpdateTest()
        {
            // The user hit backspace to the beginning
            if (_userText.Length == 0)
            {
                // Reset the runs
                _runs.Clear();
                _runs.Add(BuildRun(Text[0].ToString(), RunType.Current));
                _runs.Add(BuildRun(Text[1..], RunType.Untyped));

                TestPanelVM.TypingErrors = 0;
                TestPanelVM.WordsTyped = 0;
                if (_user.Mode == 1)
                {
                    TestPanelVM.ModeData = TestModel.WordCount.ToString();
                }
            }
            // The user entered a new character
            else if (_userText.Length == _previousUserText.Length + 1)
            {
                // Modify the list of runs
                Run untyped = BuildRun(_runs[^1].Text[1..], RunType.Untyped);
                _runs[^1] = BuildRun(_runs[^1].Text[0], RunType.Current);
                _runs.Add(untyped);
                Run lastChar;
                if (_userText[^1] == _text[_userText.Length - 1])
                {
                    lastChar = BuildRun(_userText[^1], RunType.Right);
                }
                else
                {
                    lastChar = BuildRun(Text[_userText.Length - 1], RunType.Wrong);
                    _errorPositions.Add(_userText.Length - 1);
                    TestPanelVM.TypingErrors++;

                    if (Text[_userText.Length - 1] != ' ')
                    {
                        _correctWordMap[TestPanelVM.WordsTyped] = false;
                    }
                }

                _runs[^3] = lastChar;

                // Word Count
                if (Text[_userText.Length - 1] == ' ')
                {
                    TestPanelVM.WordsTyped++;

                    if (_correctWordMap[TestPanelVM.WordsTyped - 1])
                    {
                        CorrectWords++;
                    }

                    if (_user.Mode == 1)
                    {
                        TestPanelVM.ModeData = (int.Parse(TestPanelVM.ModeData) - 1).ToString();
                    }

                    // Reset the flag for the next word
                    _correctWordMap[TestPanelVM.WordsTyped] = true;
                }
            }
            // The user hit backspace
            else if (_userText.Length == _previousUserText.Length - 1)
            {
                Run untyped = BuildRun(_text[(_runs.Count - 2)..], RunType.Untyped);
                _runs.RemoveAt(_runs.Count - 1);
                _runs[^1] = untyped;
                _runs[^2] = BuildRun(_text[_runs.Count - 2], RunType.Current);

                // Typing errors
                if (_errorPositions.Count > 0 && _errorPositions[^1] == _userText.Length)
                {
                    _errorPositions.RemoveAt(_errorPositions.Count - 1);
                    TestPanelVM.TypingErrors--;
                }

                // Word Count
                if (Text[_userText.Length - 1] == ' ')
                {
                    _correctWordMap[TestPanelVM.WordsTyped] = false;
                    TestPanelVM.WordsTyped--;

                    if (CorrectWords > 0)
                    {
                        CorrectWords--;
                    }

                    if (_user.Mode == 1)
                    {
                        TestPanelVM.ModeData = (int.Parse(TestPanelVM.ModeData) + 1).ToString();
                    }

                }
            }
            RichTextBlock.Document.Blocks.Clear();
            Paragraph paragraph = new Paragraph();
            foreach (Run run in _runs)
            {
                paragraph.Inlines.Add(run);
            }
            RichTextBlock.Document.Blocks.Add(paragraph);
        }

        internal void Unpause()
        {
            _isPaused = false;
        }

        internal void Pause()
        {
            _isPaused = true;
        }

        public void ResetCorrectWords()
        {
            CorrectWords = 0;
        }
    }
}
