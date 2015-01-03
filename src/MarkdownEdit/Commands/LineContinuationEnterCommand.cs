﻿using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace MarkdownEdit
{
    public class LineContinuationEnterCommand : ICommand
    {
        private readonly TextEditor _editor;
        private readonly ICommand _baseCommand;
        private readonly Regex _orderedListPattern = new Regex("^[ ]{0,3}(\\d+)([\\.\\)])(?=[ ]{1,3}\\S)", RegexOptions.Compiled);
        private readonly Regex _orderedListEndPattern = new Regex("^[ ]{0,3}(\\d+)([\\.\\)])(?=[ ]{1,3}\\s*)", RegexOptions.Compiled);
        private readonly Regex _unorderedListPattern = new Regex("^[ ]{0,3}[-\\*\\+](?=[ ]{1,3}\\S)", RegexOptions.Compiled);
        private readonly Regex _unorderedListEndPattern = new Regex("^[ ]{0,3}[-\\*\\+](?=[ ]{1,3}\\s*)", RegexOptions.Compiled);
        private readonly Regex _blockQuotePattern = new Regex("^(([ ]{0,4}>)+)[ ]{0,4}.{2}", RegexOptions.Compiled);
        private readonly Regex _blockQuoteEndPattern = new Regex("^([ ]{0,4}>)+[ ]{0,4}\\s*", RegexOptions.Compiled);

        public LineContinuationEnterCommand(TextEditor editor, ICommand baseCommand)
        {
            _editor = editor;
            _baseCommand = baseCommand;
        }

        public bool CanExecute(object parameter)
        {
            return _editor.TextArea != null && _editor.TextArea.IsKeyboardFocused;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            _baseCommand.Execute(parameter);

            var line = _editor.Document.GetLineByOffset(_editor.SelectionStart)?.PreviousLine;
            if (line == null) return;
            var text = _editor.Document.GetText(line.Offset, line.Length);

            if (_unorderedListPattern.Match(text).Success)
            {
                var match = _unorderedListPattern.Match(text);
                _editor.Document.Insert(_editor.SelectionStart, match.Groups[0].Value.Trim() + " ");
            }
            else if (_unorderedListEndPattern.Match(text).Success)
            {
                _editor.Document.Remove(line);
            }
            else if (_orderedListPattern.Match(text).Success)
            {
                int number;
                var match = _orderedListPattern.Match(text);
                if (int.TryParse(match.Groups[1].Value, out number))
                {
                    _editor.Document.Insert(_editor.SelectionStart, string.Format("{0}{1} ", ++number, match.Groups[2].Value.Trim()));
                }
            }
            else if (_orderedListEndPattern.Match(text).Success)
            {
                _editor.Document.Remove(line);
            }
            else if (_blockQuotePattern.Match(text).Success)
            {
                var match = _blockQuotePattern.Match(text);
                _editor.Document.Insert(_editor.SelectionStart, match.Groups[1].Value.TrimStart());
            }
            else if (_blockQuoteEndPattern.Match(text).Success)
            {
                _editor.Document.Remove(line);
            }
        }
    }
}