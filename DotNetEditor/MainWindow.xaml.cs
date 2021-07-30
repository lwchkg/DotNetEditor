using System;
using System.Windows;
using System.Windows.Input;

namespace DotNetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string untitledFile = "[Untitled]";

        private CodeRunner.CodeRunnerBase _codeRunner;
        private TextBuffer.TextBuffer _currentBuffer;
        private readonly FileUtils.FileHandler _fileHandler;

        public MainWindow()
        {
#if DEBUG
            App.client.Notify(new ArgumentException("Bugsnag crash report testing"));
#endif
            InitializeComponent();
            _codeRunner = null;
            _currentBuffer = new TextBuffer.TextBuffer(null, TextEditor.Document,
                                                       TextBuffer.FileMode.VB);
            _fileHandler = new FileUtils.FileHandler(this, TextEditor.Load, TextEditor.Save);

            // Load sample code (in VB)
            TextEditor.Text = Properties.Resources.samplecode;
            _currentBuffer.IsDirty = false;
            UpdateTitle();
            buttonCodeTypeVB.IsChecked = true;

            NSImports.Text = String.Join(Environment.NewLine,
                                         CodeRunner.CodeRunnerBase.DefaultNSImports);
            AssemblyImports.Text = String.Join(Environment.NewLine,
                                               CodeRunner.CodeRunnerBase.DefaultAssemblyImports);

            AppCommands.Run.Execute(null, this);
        }

        private void UpdateTitle()
        {
            Title = String.Format("{0}{1} - {2}", _currentBuffer.IsDirty ? "*" : "",
                System.IO.Path.GetFileName(_currentBuffer.Filename ?? untitledFile), App.AppName);
        }

        private void UpdateHighlighter()
        {
            TextEditor.SyntaxHighlighting =
                ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition(
                    _currentBuffer.GetLanguageAsString());
        }

        private void buttonWordWrap_Click(object sender, RoutedEventArgs e)
        {
            OutputArea.WordWrap = buttonWordWrap.IsChecked == true;
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (_currentBuffer != null && !_currentBuffer.IsDirty)
            {
                _currentBuffer.IsDirty = true;
                UpdateTitle();
            }
        }

        private string GetSaveFilter(TextBuffer.FileMode mode)
        {
            if (mode == TextBuffer.FileMode.VB)
            {
                return "VB Program|*.vb|All Files|*.*";
            }
            else
            {
                return "C# Program|*.cs|All Files|*.*";
            }
        }

        private string GetOpenFilter()
        {
            return "All Supported Files|*.vb; *.cs|VB Program|*.vb|C# Program|*.cs|All Files|*.*";
        }

        #region Commands
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TextEditor == null ||
                           _currentBuffer.IsDirty ||
                           _currentBuffer.Filename == null;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentBuffer.Filename == null)
            {
                ApplicationCommands.SaveAs.Execute(sender, this);
                return;
            }

            FileUtils.FileHandler.FileOperationResult result =
                _fileHandler.Save(_currentBuffer.Filename);
            if (result.Success)
            {
                _currentBuffer.IsDirty = false;
                UpdateTitle();
            }
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileUtils.FileHandler.FileOperationResult result =
                _fileHandler.SaveAs(GetSaveFilter(TextBuffer.FileMode.VB));

            if (result.Success)
            {
                _currentBuffer.Filename = result.Filename;
                _currentBuffer.IsDirty = false;
                UpdateTitle();
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileUtils.FileHandler.FileOperationResult result =
                _fileHandler.LoadWithDialog(GetOpenFilter());

            if (result.Success)
            {
                _currentBuffer.Filename = result.Filename;
                _currentBuffer.IsDirty = false;
                _currentBuffer.AutoSelectLanguage();

                UpdateTitle();
                if (_currentBuffer.Language == TextBuffer.FileMode.VB)
                {
                    buttonCodeTypeVB.IsChecked = true;
                }
                else
                {
                    buttonCodeTypeCS.IsChecked = true;
                }
            }
        }

        private void Run_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _codeRunner == null;
        }

        private async void Run_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_codeRunner != null)
                throw new Exception("Cannot run code while some other code is running.");

            if (_currentBuffer.Language == TextBuffer.FileMode.VB)
            {
                _codeRunner = new CodeRunner.VBCodeRunner(TextEditor.Text, InputData.Text,
                                                          OutputArea);
            }
            else
            {
                _codeRunner = new CodeRunner.CSCodeRunner(TextEditor.Text, InputData.Text,
                                                          OutputArea);
            }
            CommandManager.InvalidateRequerySuggested();
            Cursor = Cursors.Wait;
            ForceCursor = true;

            _codeRunner.NSImports = NSImports.Text.Split(new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            _codeRunner.AssemblyImports = AssemblyImports.Text.Split(new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            bool success = await _codeRunner.Run();
            OutputArea.WordWrap = !success || buttonWordWrap.IsChecked == true;

            _codeRunner = null;
            CommandManager.InvalidateRequerySuggested();
            Cursor = Cursors.Arrow;
            ForceCursor = false;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _codeRunner != null;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _codeRunner.Terminate();
        }

        private void About_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutBox().ShowDialog();
        }
#endregion

        private void buttonCodeTypeVB_Checked(object sender, RoutedEventArgs e)
        {
            _currentBuffer.Language = TextBuffer.FileMode.VB;
            UpdateHighlighter();
        }

        private void buttonCodeTypeCS_Checked(object sender, RoutedEventArgs e)
        {
            _currentBuffer.Language = TextBuffer.FileMode.CS;
            UpdateHighlighter();
        }
    }
}
