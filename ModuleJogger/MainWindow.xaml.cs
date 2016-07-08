using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
//using System.Xml;

// This is for Antlr, the parser generator found at antlr.org
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

// This is for Roslyn, the Microsoft open source C# (and VB) code analyzer
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// This is for AlphaFS, which is a drop-in replacement for File and Directory, etc., .Net classes
// which enables support for long paths (>32,000 characters), among other things.
//using Alphaleonis.Win32.Filesystem;
using File = Alphaleonis.Win32.Filesystem.File;
using Directory = Alphaleonis.Win32.Filesystem.Directory;

using static System.Console;

//using Microsoft.Win32;
// This is an alternate regular expression engine
//using PCRE;            
//using System.Xml;

namespace ModuleJogger
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // This object is to store the functionList.xml file
        //XmlDocument xmlFunctionList = new XmlDocument();
        // This is a regular expression to match C# methods
        //private string regexCSharpFunction;
        //private string regexCCPPFunction1;
        //private string regexCCPPFunction2;

        // This variable will be true when the file processing task is active.
        private bool _mRunning = false;
        // This token is when the Cancel button is pressed.
        private CancellationTokenSource _mCancelTokenSource = null;
        // This contains the file count for the relevant files in the directory tree.
        private readonly NumberOfFiles _numberOfFiles = new NumberOfFiles();
        // This is a List of modules, which is used as the data backing for DataGrid.
        // Use ObservableCollection<T> so that the datagrid will automatically update, as the data changes.
        //private List<Module> _moduleList; // = new List<Module>();
        private ObservableCollection<Module> _moduleList;
        // This is a List of Module objects, to be used as a queue for the UI thread, to be periodically dumped into _moduleList.
        //private List<Module> queueList;
        // FileCountTrigger files processed queue length, which determines how often (after FileCountTrigger files) PeriodicProcess() is called.
        private const int FileCountTrigger = 100;
        // This is the base directory started from.
        private string _baseDirectory;
        // This is used to store a List of the error texts, so it can be displayed to the user at the end of the processing.
        private List<string> _errorList;
        // Number of rows to allow the DataGrid to grow before issuing a Refresh() to update the UI.
        // Must be >= 1, otherwise there will be a divide by zero exception.
        //private const int refreshCount = 1;
        // Number of nested folder levels.
        private int _numberOfNestedFolderLevels = 0;

        // enum containing our supported (or future possible supported) languages.
        public enum Languages { C, Cobol, Cpp, CSharp, Java, JavaScript, Perl, Python };

        // I changed the following from List<string> to List<StringCollection>, so that the custom file extensions
        // could be saved to user.config and thereby persisted across application runs.
        List<StringCollection> _languageExtensions = new List<StringCollection>();
        // List of custom extensions (or file name endings--e.g., .h.in) for Cpp parsing.
        /*
        List<string>[] _languageExtensions = new List<string>[]
        {
            new List<string> { ".c", ".h" },
            new List<string> { ".cbl", ".cob" },   //.cpy is another possible but unlikely Cobol file extension
            //new List<string> { ".cpp", ".hpp" },
            new List<string> { ".cs" },
            new List<string> { ".java" },
            new List<string> { ".js" },
            new List<string> { ".pl" },
            new List<string> { ".py" }
        };
        */

        // This will suppress all WriteLine statements if set to false.
        public static readonly bool IsDebugging = false;

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the file extensions from the User settings.
            _languageExtensions.Add(Properties.Settings.Default.LanguageCExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageCOBOLExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageCppExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageCSharpExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageJavaExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageJavaScriptExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguagePerlExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguagePythonExtensions);

            // Put the program version in the program description
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            textBlockProgramVersion.Text = "64 bit\nVersion " + version;
            if (textBoxFolderName != null && !string.IsNullOrEmpty(textBoxFolderName.Text))
                buttonClear.IsEnabled = true;

            //dataGrid.ItemsSource = _moduleList;

            /*
            // This code to read in an XML file and get a particular value is now not used, but
            // is left here for reference purposes.
            xmlFunctionList.Load("functionList.xml");
            //XmlNode node = xmlFunctionList.DocumentElement.SelectSingleNode("cs_function");
            WriteLine("Examining...");
            foreach (XmlNode node in xmlFunctionList.DocumentElement.ChildNodes)
                foreach (XmlNode nodeFunction in node)
                {
                    if (nodeFunction.Name == "parsers")
                        foreach (XmlNode nodeParser in nodeFunction)
                        {
                            WriteLine(nodeParser.Name);
                            WriteLine(nodeParser.Attributes["id"].InnerText);
                            WriteLine(nodeParser.InnerText);
                            if (nodeParser.Attributes["id"].InnerText == "cs_function")
                            {
                                XmlNode nodeInner = nodeParser["classRange"]["function"];
                                regexCSharpFunction = nodeInner.Attributes["mainExpr"].InnerText;
                                WriteLine("regexCSharpFunction = " + regexCSharpFunction);
                                ReadLine();
                            }
                            if (nodeParser.Attributes["id"].InnerText == "c_cpp_function")
                            {
                                XmlNode nodeInner = nodeParser["classRange"]["function"];
                                regexCCPPFunction1 = nodeInner.Attributes["mainExpr"].InnerText;
                                WriteLine("regexCCPPFunction1 = " + regexCCPPFunction1);
                                ReadLine();
                            }
                            if (nodeParser.Attributes["id"].InnerText == "c_cpp_function")
                            {
                                XmlNode nodeInner = nodeParser["function"];
                                regexCCPPFunction2 = nodeInner.Attributes["mainExpr"].InnerText;
                                WriteLine("regexCCPPFunction2 = " + regexCCPPFunction2);
                                ReadLine();
                            }
                        }
                }
                */
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            /*
            try
            {
                var UserConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                WriteLine(UserConfig.FilePath);
            }
            catch (ConfigurationException ex)
            { }
            Properties.Settings.Default.Save();
            */
        }

        // This is the Open File button event.
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            //openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //if (openFileDialog.ShowDialog() ?? false)
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                    //listBoxFiles.Items.Add(Path.GetFileName(filename));
                    listBoxFiles.Items.Add(filename);
            }
        }

        // This is the Remove Row button event.
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = listBoxFiles.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected)
                listBoxFiles.Items.Remove(item);
        }

        // This method is called when a selection change event in the listBoxFiles component occurs.
        // If there is something selected, the Remove Row button is enabled.
        void DeleteButtonCheck(object sender, SelectionChangedEventArgs args)
        {
            //ListBoxItem lbi = ((sender as System.Windows.Forms.ListBox).SelectedItem as ListBoxItem);
            if (listBoxFiles.SelectedItems.Count > 0)
                buttonDelete.IsEnabled = true;
            else
                buttonDelete.IsEnabled = false;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxFolderName.Text = "";
        }

        void ClearButtonCheck(object sender, TextChangedEventArgs args)
        {
            if (buttonClear != null)
                //if (textBoxFolderName?.Text.Length > 0)
                if (textBoxFolderName.Text.Length > 0)
                    buttonClear.IsEnabled = true;
                else
                    buttonClear.IsEnabled = false;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void checkBox_Copy_Checked(object sender, RoutedEventArgs e)
        {

        }

        // This opens the Select Folder dialog.
        private void buttonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            // Some docs suggeset a RootFolder ought to be specified, but it doesn't appear to work,
            // (it only shows the Desktop folder, and nothing else,) so I commented it out.
            //folderBrowserDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
            folderBrowserDialog.SelectedPath = textBoxFolderName.Text;
            // Show the FolderBrowserDialog.  Use a customized class though to show the
            // selected folder, which whill attempt to scroll into focus the selected folder.
            //DialogResult result = folderBrowserDialog.ShowDialog();
            DialogResult result = FolderBrowserLauncher.ShowFolderBrowser(folderBrowserDialog);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBoxFolderName.Text = folderBrowserDialog.SelectedPath;
                // Set in the XAML:
                // Text="{Binding Source={StaticResource Settings}, Path=Default.RootFolder, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                // The PropertyChanged UpdateSourceTrigger made the following unnecesary:
                //ModuleJogger.Properties.Settings.Default["RootFolder"] = folderBrowserDialog.SelectedPath;
            }
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            CustomizeFileExtensionsWindow fileExtensionsWindow = new CustomizeFileExtensionsWindow
                    (Languages.Cpp.ToString(),
                    _languageExtensions[(int)Languages.Cpp]);
            fileExtensionsWindow.ShowDialog();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            //Properties.Settings.Default.Save();
            //Properties.Settings.Default.Reload();

            // This doesn't change the in-memory StringCollections (for whatever reason)
            // so restore them here:
            _languageExtensions = new List<StringCollection>();
            _languageExtensions.Add(Properties.Settings.Default.LanguageCExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageCOBOLExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageCppExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageCSharpExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageJavaExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguageJavaScriptExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguagePerlExtensions);
            _languageExtensions.Add(Properties.Settings.Default.LanguagePythonExtensions);

            System.Windows.MessageBox.Show("Default settings, including custom file endings, are now restored.", "Module Jogger",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // This begins the main parsing process.
        private async void buttonGO_Click(object sender, RoutedEventArgs e)
        {
            if (!_mRunning)
            {
                // Remove TextBlock trigger:
                //textBlockNested.Style.Triggers.Clear();
                textBlockNested.Style = null;
                labelCurrentFileCount.Style = null;
                // To turn back on the fade-in style, this could be used:
                //textBlockNested.Style = this.FindResource("FadeInStyle") as Style;
                //labelCurrentFileCount.Style = this.FindResource("FadeInStyle") as Style;

                // Initialize current run by resetting values.
                _mRunning = true;
                labelProgress.Content = "Counting Files";
                buttonGO.Content = "Cancel";
                _numberOfFiles.Count = 0;
                //_moduleList = new List<Module>();
                _numberOfNestedFolderLevels = (int)comboBoxNestedFolders.SelectedItem;
                //if (_numberOfNestedFolderLevels == 0)
                //    _moduleList = new ObservableCollection<Module>();
                //else
                //    _moduleList = new ObservableCollection<ModuleSubgrouped>();
                _moduleList = new ObservableCollection<Module>();
                //queueList = new List<Module>();
                dataGrid.ItemsSource = null;
                //dataGrid.ItemsSource = _moduleList;
                listBoxFunctionList.Items.Clear();
                _baseDirectory = null;
                _errorList = new List<string>();
                labelErrorCount.Content = "0";
                labelNumberOfFiles.Content = "0";
                progressBar.Value = 0;
                textBoxCurrentFile.Text = "";
                // Disable UI components that ought not to change during a run.
                buttonSelectFiles.IsEnabled = false;
                // Save button states for the two clear buttons
                bool wasButtonDeleteEnabled = buttonDelete.IsEnabled;
                bool wasButtonClearEnabled = buttonClear.IsEnabled;
                buttonDelete.IsEnabled = false;
                buttonSelectFolder.IsEnabled = false;
                buttonClear.IsEnabled = false;
                comboBoxNestedFolders.IsEnabled = false;
                buttonReset.IsEnabled = false;
                //checkBoxCPP.Style = Resources["CheckBoxFadeInStyle"] as Style;
                var story = (Storyboard)this.FindResource("FadeInStoryboard") as Storyboard;
                if (story != null)
                {
                    Storyboard.SetTarget(story, this.checkBoxCPP);
                    story.Begin(checkBoxCPP, true);
                    Storyboard.SetTarget(story, this.checkBoxCS);
                    story.Begin(checkBoxCS, true);
                    Storyboard.SetTarget(story, this.checkBoxJava);
                    story.Begin(checkBoxJava, true);
                    Storyboard.SetTarget(story, this.checkBoxJavaScript);
                    story.Begin(checkBoxJavaScript, true);
                    //(this.Resources["FadeInStoryboard"] as Storyboard).Begin(checkBoxCPP);
                    //(this.Resources["FadeInStoryboard"] as Storyboard).Begin(checkBoxCS);
                    //(this.Resources["FadeInStoryboard"] as Storyboard).Begin(checkBoxJava);
                    //(this.Resources["FadeInStoryboard"] as Storyboard).Begin(checkBoxJavaScript);
                }

                // Create progress and cancel objects
                Progress<int> progress = new Progress<int>(SetProgress);
                _mCancelTokenSource = new CancellationTokenSource();
                // First, count the files
                // Process individually selected files first.
                foreach (string selectedFile in listBoxFiles.Items)
                    await TraverseTree(progress, _mCancelTokenSource.Token, selectedFile, true);
                // Then process a folder tree, if selected.
                if (Directory.Exists(textBoxFolderName.Text))
                {
                    _baseDirectory = textBoxFolderName.Text;
                    await TraverseTree(progress, _mCancelTokenSource.Token, _baseDirectory, true);
                }

                labelNumberOfFiles.Content = _numberOfFiles.Count;
                // Set up the progressBar constraints
                progressBar.Minimum = _numberOfFiles.Count > 0 ? 1 : 0;
                progressBar.Maximum = _numberOfFiles.Count;
                labelProgress.Content = "Processing";
                // Now that we have the number of files, we know how to set up the Progress
                try
                {
                    // Reset the _numberOfFiles object
                    _numberOfFiles.Count = 0;
                    // Launch the process. After launching, will "return" from this method.
                    // Process individually selected files first.
                    foreach (string selectedFile in listBoxFiles.Items)
                        await TraverseTree(progress, _mCancelTokenSource.Token, selectedFile, false);
                    // Then process a folder tree, if selected.
                    if (!string.IsNullOrEmpty(_baseDirectory) && Directory.Exists(_baseDirectory))
                        await TraverseTree(progress, _mCancelTokenSource.Token, _baseDirectory, false);

                    // But after complete, processing will continue here
                    // This is 1 of 2 locations where end processing occurs (see next block for 2nd).
                    labelProgress.Content = "Done";
                    RemoveWhitespace();
                    if (_numberOfNestedFolderLevels > 0)
                        dataGrid.ItemsSource = _moduleList.Cast<ModuleSubgrouped>();
                    else
                        dataGrid.ItemsSource = _moduleList;
                }
                catch (OperationCanceledException)
                {
                    // If the operation was cancelled, the exception will be thrown as though it came from the await line
                    // This is 2 of 2 locations where end processing occurs (see previous block for 1st).
                    labelProgress.Content = "Canceled";
                    RemoveWhitespace();
                    if (_numberOfNestedFolderLevels > 0)
                        dataGrid.ItemsSource = _moduleList.Cast<ModuleSubgrouped>();
                    else
                        dataGrid.ItemsSource = _moduleList;
                    _mRunning = false;
                }
                finally
                {
                    // Reset the UI
                    buttonGO.Content = "GO";
                    _mRunning = false;
                    _mCancelTokenSource = null;
                    // Enable UI components that ought not to change during a run.
                    buttonSelectFiles.IsEnabled = true;
                    buttonDelete.IsEnabled = wasButtonDeleteEnabled;
                    buttonSelectFolder.IsEnabled = true;
                    buttonClear.IsEnabled = wasButtonClearEnabled;
                    comboBoxNestedFolders.IsEnabled = true;
                    buttonReset.IsEnabled = true;
                    if (story != null)
                    {
                        (this.Resources["FadeInStoryboard"] as Storyboard).Stop(checkBoxCPP);
                        (this.Resources["FadeInStoryboard"] as Storyboard).Stop(checkBoxCS);
                        (this.Resources["FadeInStoryboard"] as Storyboard).Stop(checkBoxJava);
                        (this.Resources["FadeInStoryboard"] as Storyboard).Stop(checkBoxJavaScript);
                    }
                }
            }
            else
            {
                // User hit the Cancel button, so signal the cancel token and put a temporary message in the UI
                labelProgress.Content = "Waiting to cancel...";
                _mCancelTokenSource.Cancel();
            }

        }

        // For some reason, newlines followed by spaces are being put into the ModuleName cells of very long
        // cells.  Remove that extra whitespace here.
        private void RemoveWhitespace()
        {
            // To remove whitespace with LINQ:
            //myString = new string(myString.Where(c => !char.IsWhiteSpace(c)).ToArray());
            // To remove whitespace with Regex:
            //Regex.Replace(XML, @"\s+", "");
            foreach (var module in _moduleList)
            {
                //module.Path = Regex.Replace(module.Path, @"\s+", "");
                //module.FileName = Regex.Replace(module.FileName, @"\s+", "");
                // Only strip the whitespace from ModuleName, as whitespace can legitimately appear
                // in the other fields.
                //module.ModuleName = Regex.Replace(module.ModuleName, @"\s+", "");
                if (!string.IsNullOrEmpty(module.ModuleName))
                    module.ModuleName = module.ModuleName.Trim();
            }

        }

        /// <summary>Updates the progress display</summary>
        /// <param name="value">The new progress value</param>
        private void SetProgress(int value)
        {
            // Add 1 so that progress is "completed"
            int adjustedValue = value + 1;

            // Make sure value is in range
            adjustedValue = Math.Max(adjustedValue, (int)progressBar.Minimum);
            adjustedValue = Math.Min(adjustedValue, (int)progressBar.Maximum);

            progressBar.Value = adjustedValue;
        }

        public class ReverseComparer : IComparer
        {
            // Call CaseInsensitiveComparer.Compare with the parameters reversed.
            public int Compare(Object x, Object y)
            {
                return (new CaseInsensitiveComparer()).Compare(y, x);
            }
        }

        // This method iteratively traverses a directory structure, processing individual files where necessary.
        public Task TraverseTree(IProgress<int> progress, CancellationToken ct, string targetPath, bool countingOnly)
        {
            return Task.Run(() =>
            {
                // The following is a modification of the iterative method of directory traversal found at 
                // https://msdn.microsoft.com/en-us/library/bb513869.aspx

                // Data structure to hold names of subfolders to be
                // examined for files.
                Stack<string> dirs = new Stack<string>(20);

                // If this path isn't a directory, then it's one of the selected files.
                if (!Directory.Exists(targetPath))
                {
                    //throw new ArgumentException();
                    _numberOfFiles.Increment();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        labelCurrentFileCount.Content = _numberOfFiles.Count;
                    }));
                    if (!countingOnly)
                    {
                        ProcessFile(targetPath);
                        progress.Report(_numberOfFiles.Count);

                    }
                }
                else
                {
                    // The path passed in is a directory, so process the entire directory.
                    dirs.Push(targetPath);

                    while (dirs.Count > 0 && !ct.IsCancellationRequested)
                    {
                        string currentDir = dirs.Pop();
                        string[] subDirs;
                        try
                        {
                            // Incomplete LINQ way to sort:
                            //subDirs = Directory.GetDirectories(currentDir).OrderBy(f => f);
                            subDirs = Directory.GetDirectories(currentDir);
                            // Just sort with the Array.Sort method:
                            Array.Sort(subDirs, new CaseInsensitiveComparer());
                            // Sort the array in descending order, so that they will get popped off the stack in the right order.
                            Array.Reverse(subDirs);
                        }
                        // An UnauthorizedAccessException exception will be thrown if we do not have
                        // discovery permission on a folder or file. It may or may not be acceptable 
                        // to ignore the exception and continue enumerating the remaining files and 
                        // folders. It is also possible (but unlikely) that a DirectoryNotFound exception 
                        // will be raised. This will happen if currentDir has been deleted by
                        // another application or thread after our call to Directory.Exists. The 
                        // choice of which exceptions to catch depends entirely on the specific task 
                        // you are intending to perform and also on how much you know with certainty 
                        // about the systems on which this code will run.
                        catch (UnauthorizedAccessException e)
                        {
                            if (IsDebugging)
                            {
                                string message = e.Message + "\n   for path " + currentDir;
                                WriteLine(message);
                            }
                            //_errorList.Add(message);
                            //Dispatcher.BeginInvoke(new Action(() =>
                            //{
                            //    labelErrorCount.Content = _errorList.Count;
                            //}));
                            continue;
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            if (IsDebugging)
                                WriteLine(e.Message);
                            continue;
                        }
                        catch (PathTooLongException e)
                        {
                            string message = e.Message + "\n   for path " + currentDir;
                            if (IsDebugging)
                                WriteLine(message);
                            if (!countingOnly)
                            {
                                _errorList.Add(message);
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    labelErrorCount.Content = _errorList.Count;
                                }));
                            }
                            continue;
                        }

                        string[] files;
                        try
                        {
                            files = Directory.GetFiles(currentDir);
                            Array.Sort(files, new CaseInsensitiveComparer());
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            if (IsDebugging)
                            {
                                string message = e.Message + "\n   for path " + currentDir;
                                WriteLine(message);
                            }
                            //_errorList.Add(message);
                            //Dispatcher.BeginInvoke(new Action(() =>
                            //{
                            //    labelErrorCount.Content = _errorList.Count;
                            //}));
                            continue;
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            if (IsDebugging)
                                WriteLine(e.Message);
                            continue;
                        }
                        // Perform the required action on each file here.
                        // Modify this block to perform your required task.
                        foreach (string file in files)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                PeriodicProcess();
                                break;
                            }
                            try
                            {
                                // Perform whatever action is required in your scenario.
                                //FileInfo fi = new FileInfo(file);
                                //WriteLine("{0}: {1}, {2}", fi.Name, fi.Length, fi.CreationTime);
                                _numberOfFiles.Increment();
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    labelCurrentFileCount.Content = _numberOfFiles.Count;
                                }));
                                if (!countingOnly)
                                {
                                    ProcessFile(file);
                                    progress.Report(_numberOfFiles.Count);
                                    if (_numberOfFiles.Count % FileCountTrigger == 0)
                                    {
                                        PeriodicProcess();
                                    }
                                }
                            }
                            catch (FileNotFoundException e)
                            {
                                // If file was deleted by a separate application
                                //  or thread since the call to TraverseTree()
                                // then just continue.
                                if (IsDebugging)
                                    WriteLine(e.Message);
                            }
                        }

                        // Push the subdirectories onto the stack for traversal.
                        // This could also be done before handling the files.
                        foreach (string str in subDirs)
                            dirs.Push(str);
                    }
                    if (_errorList.Count > 0 && !countingOnly)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ErrorResultWindow errorWindow = new ErrorResultWindow(_errorList);
                            errorWindow.Title += ": " + _errorList.Count + " Errors Encountered";
                            errorWindow.ShowDialog();
                            //if (errorWindow.ShowDialog())
                            //perhaps do something here such as save the error list to a file if necessary
                            //if (errorWindow.Answer) ...
                        }));
                    }
                }
            }, ct);
        }

        private void PeriodicProcess()
        {
            // For garbage collection to clear up parse object information; otherwise, memory usgae
            // will climb to 6 GB and beyond with large trees, on Windows Server 2008 R2 64-bit.
            // (The problem doesn't seem to occur on Windows 10 Pro 64-bit.)
            // This may not be necessary though now that I'm not setting the DataGrid's data source
            // until after processing is complete.
            GC.Collect();
            
            /*
            foreach (var module in queueList)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _moduleList.Add(module);
                }));
            }
            queueList.Clear();
            */
            /*
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //_moduleList.AddRange(queueList.ToList());
                //_moduleList.ToList().ForEach(queueList.Add);
                //queueList.ForEach(_moduleList.Add);
                try
                {
                    dataGrid.ScrollIntoView(_moduleList[_moduleList.Count - 1]);
                }
                catch (ArgumentOutOfRangeException e) { }
            }));
            */
        }

        /*
        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        // Recursing directories with this recursive method (there an echo in here LOL?) was replaced
        // with an iterative algorithm, because this Task-based recursive method didn't work properly.
        private Task ProcessDirectory(IProgress<int> progress, CancellationToken ct, string targetDirectory, bool countingOnly)
        {
            return Task.Run(() =>
            {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                {
                    _numberOfFiles.Increment();
                    if (!countingOnly)
                    {
                        ProcessFile(fileName);
                        progress.Report(_numberOfFiles.Count);
                    }
                }

                // Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                    ProcessDirectory(progress, ct, subdirectory, countingOnly);
                    //Task.Factory.StartNew(() =>
                    //    ProcessDirectory(progress, ct, targetDirectory, countingOnly)
                    //);
            }, ct);
        }
        */

        // Process an individual file.
        public void ProcessFile(string path)
        {
            // Convert to all lower case, so that case-insensitive comparisons can 
            // be made with the file extensions.
            string pathLowerCase = path.ToLower();
            string stringContents;

            //TODO:  It may be necessary to check for file extensions that are mapped to more than one language.

            // Process C# files.
            // The checkboxes must be checked in another thread.  Might as well check them during processing, in case the user would like
            // to fiddle with the checkboxes during the processing.  Also, instead of having to manually check each file extension
            // in the _languageExtensions array of Lists, with path.EndsWith(".cs") for example, we can use LINQ to check them all at once
            // (the commented out lines had to be modified when List<string[]> was changed to List<StringCollection> for settings
            // persistence, which thankfully wasn't a huge problem, since I was easily able to Cast the StringCollection to Array):
            try
            {
                if ((checkBoxCS.Dispatcher.Invoke(() => checkBoxCS.IsChecked) == true &&
                     //_languageExtensions[(int)Languages.CSharp].Any(x => pathLowerCase.EndsWith(x)))
                     _languageExtensions[(int) Languages.CSharp].Cast<string>()
                         .ToArray()
                         .Any(x => pathLowerCase.EndsWith(x)))
                    || (checkBoxCPP.Dispatcher.Invoke(() => checkBoxCPP.IsChecked) == true &&
                        //_languageExtensions[(int)Languages.Cpp].Any(x => pathLowerCase.EndsWith(x)))
                        _languageExtensions[(int) Languages.Cpp].Cast<string>()
                            .ToArray()
                            .Any(x => pathLowerCase.EndsWith(x)))
                    || (checkBoxJava.Dispatcher.Invoke(() => checkBoxJava.IsChecked) == true &&
                        //_languageExtensions[(int)Languages.Java].Any(x => pathLowerCase.EndsWith(x)))
                        _languageExtensions[(int) Languages.Java].Cast<string>()
                            .ToArray()
                            .Any(x => pathLowerCase.EndsWith(x)))
                    || (checkBoxJavaScript.Dispatcher.Invoke(() => checkBoxJavaScript.IsChecked) == true &&
                        //_languageExtensions[(int)Languages.JavaScript].Any(x => pathLowerCase.EndsWith(x))))
                        _languageExtensions[(int) Languages.JavaScript].Cast<string>()
                            .ToArray()
                            .Any(x => pathLowerCase.EndsWith(x))))
                {
                    string fileName = Path.GetFileName(path);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        textBoxCurrentFile.Text = fileName;
                    }));
                    try
                    {
                        // Am using AlphaFS, a drop-in replacement for .Net classes such as File and Directory,
                        // which allows using long paths:
                        string longPath = @"\\?\" + path;
                        stringContents = File.ReadAllText(longPath);
                        string relPath;
                        StringBuilder nestedPathBuilder = new StringBuilder();
                        string nestedPath = null;
                        if (_baseDirectory != null)
                        {
                            Uri fullPath = new Uri(path, UriKind.Absolute);
                            Uri relRoot = new Uri(_baseDirectory, UriKind.Absolute);
                            relPath = relRoot.MakeRelativeUri(fullPath).ToString();
                            for (int i = 0; i < _numberOfNestedFolderLevels && relPath.IndexOf('/') != -1; i++)
                            {
                                nestedPathBuilder.Append(relPath.Substring(0, relPath.IndexOf('/') + 1));
                                relPath = relPath.Substring(relPath.IndexOf('/') + 1);
                            }
                            nestedPath = nestedPathBuilder.ToString();
                            nestedPath = nestedPath.Replace("/", @"\");
                            //relPath.Replace("/", "\\");  //or:
                            relPath = relPath.Replace("/", @"\");
                            // Remove the file name from the path:
                            relPath = relPath.Replace(fileName, "");
                        }
                        else
                            relPath = ".";

                        //ParseRegex(stringContents);
                        //ParseAntlr(stringContents);

                        // Use the proper parser corresponding to the language:
                        //if (_languageExtensions[(int)Languages.CSharp].Any(x => pathLowerCase.EndsWith(x)))
                        if (
                            _languageExtensions[(int) Languages.CSharp].Cast<string>()
                                .ToArray()
                                .Any(x => pathLowerCase.EndsWith(x)))
                            ParseRoslyn(nestedPath, relPath, fileName, stringContents);
                        //if (_languageExtensions[(int)Languages.Cpp].Any(x => pathLowerCase.EndsWith(x)))
                        if (
                            _languageExtensions[(int) Languages.Cpp].Cast<string>()
                                .ToArray()
                                .Any(x => pathLowerCase.EndsWith(x)))
                            ParseAntlrCpp(nestedPath, relPath, fileName, stringContents);
                        //if (_languageExtensions[(int)Languages.Java].Any(x => pathLowerCase.EndsWith(x)))
                        if (
                            _languageExtensions[(int) Languages.Java].Cast<string>()
                                .ToArray()
                                .Any(x => pathLowerCase.EndsWith(x)))
                            ParseAntlrJava8(nestedPath, relPath, fileName, stringContents);
                        //if (_languageExtensions[(int)Languages.JavaScript].Any(x => pathLowerCase.EndsWith(x)))
                        if (
                            _languageExtensions[(int) Languages.JavaScript].Cast<string>()
                                .ToArray()
                                .Any(x => pathLowerCase.EndsWith(x)))
                            ParseAntlrJavaScript(nestedPath, relPath, fileName, stringContents);
                    }
                    catch (PathTooLongException e)
                    {
                        string message = e.Message + "\n   for file " + path;
                        if (IsDebugging)
                            WriteLine(message);
                        _errorList.Add(message);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            labelErrorCount.Content = _errorList.Count;
                        }));
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                // This occurs when the program is closed by clicking on the top right corner X
                // while a process Task is occurring.
                // No need to do anything here, as the program is closing.
            }
        }

        private void ParseAntlrJava8(string nestedPath, string path, string fileName, string inputText)
        {
            try
            {
                if (IsDebugging)
                    WriteLine("Entered ParseAntlrJava8 method for this file: " + fileName);
                // Replace ... in the source file with [] since the Java8 grammar doesn't seem to
                // handle (or maybe I just haven't yet looked in the right place) the ... syntax.
                inputText = inputText.Replace("...", "[]");
                AddModule(nestedPath, path, fileName, "File Level");
                //StreamReader inputStream = new StreamReader(path);
                AntlrInputStream input = new AntlrInputStream(inputText);
                Java8Lexer lexer = new Java8Lexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                Java8Parser parser = new Java8Parser(tokens);

                //lexer.RemoveErrorListeners();
                //lexer.AddErrorListener(DescriptiveErrorListener.INSTANCE);
                parser.RemoveErrorListeners();
                // Add our own error listener, which throws InvalidOperationException,
                // which is caught below.
                parser.AddErrorListener(DescriptiveErrorListener.INSTANCE);

                // According to pom.xml for this grammer, compilationUnit() is the entryPoint.
                IParseTree tree = parser.compilationUnit();
                // Don't need to create a new walker, since we aren't using a custom walker.
                //ParseTreeWalker walker = new ParseTreeWalker();
                ModuleJava8Listener moduleListener = new ModuleJava8Listener(this, parser, tokens, nestedPath, path, fileName);
                //walker.Walk(moduleListener, tree);
                ParseTreeWalker.Default.Walk(moduleListener, tree);
            }
            catch (InvalidOperationException e)
            {
                string message = e.Message + "\n   for path " + path + fileName;
                if (IsDebugging)
                    WriteLine(message);
                _errorList.Add(message);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    labelErrorCount.Content = _errorList.Count;
                }));
            }
        }

        private void ParseAntlrJavaScript(string nestedPath, string path, string fileName, string inputText)
        {
            try
            {
                if (IsDebugging)
                    WriteLine("Entered ParseAntlrJavaScript method for this file: " + fileName);
                //StreamReader inputStream = new StreamReader(path);
                AntlrInputStream input = new AntlrInputStream(inputText);
                ECMAScriptLexer lexer = new ECMAScriptLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
               ECMAScriptParser parser = new ECMAScriptParser(tokens);

                //lexer.RemoveErrorListeners();
                //lexer.AddErrorListener(DescriptiveErrorListener.INSTANCE);
                parser.RemoveErrorListeners();
                // Add our own error listener, which throws InvalidOperationException,
                // which is caught below.
                parser.AddErrorListener(DescriptiveErrorListener.INSTANCE);

                // According to pom.xml for this grammer, program() is the entryPoint.
                IParseTree tree = parser.program();
                // Don't need to create a walker, since we aren't using a custom walker.
                //ParseTreeWalker walker = new ParseTreeWalker();
                ModuleEcmaScriptListener moduleListener = new ModuleEcmaScriptListener(this, parser, tokens, nestedPath, path, fileName);
                //walker.Walk(moduleListener, tree);
                ParseTreeWalker.Default.Walk(moduleListener, tree);
            }
            catch (InvalidOperationException e)
            {
                string message = e.Message + "\n   for path " + path + fileName;
                if (IsDebugging)
                    WriteLine(message);
                _errorList.Add(message);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    labelErrorCount.Content = _errorList.Count;
                }));
            }
        }

        private void ParseAntlrCpp(string nestedPath, string path, string fileName, string inputText)
        {
            try
            {
                if (IsDebugging)
                    WriteLine("Entered ParseAntlrCpp method for this file: " + fileName);
                AddModule(nestedPath, path, fileName, "File Level");
                //StreamReader inputStream = new StreamReader(path);
                AntlrInputStream input = new AntlrInputStream(inputText);
                CPP14Lexer lexer = new CPP14Lexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                CPP14Parser parser = new CPP14Parser(tokens);

                //lexer.RemoveErrorListeners();
                //lexer.AddErrorListener(DescriptiveErrorListener.INSTANCE);
                parser.RemoveErrorListeners();
                // Add our own error listener, which throws InvalidOperationException,
                // which is caught below.
                parser.AddErrorListener(DescriptiveErrorListener.INSTANCE);

                // According to pom.xml for this grammer, translationunit is the entryPoint.
                IParseTree tree = parser.translationunit();
                // Don't need to create a walker, since we aren't using a custom walker.
                //ParseTreeWalker walker = new ParseTreeWalker();
                ModuleCppListener moduleListener = new ModuleCppListener(this, parser, tokens, nestedPath, path, fileName);
                //walker.Walk(moduleListener, tree);
                ParseTreeWalker.Default.Walk(moduleListener, tree);
            }
            catch (InvalidOperationException e)
            {
                string message = e.Message + "\n   for path " + path + fileName;
                if (IsDebugging)
                    WriteLine(message);
                _errorList.Add(message);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    labelErrorCount.Content = _errorList.Count;
                }));
            }
            // Because of C++'s complexity, and the level of nested method calls involved,
            // a StackOverflowException might occur. It does for this file:
            // Visual Studio 2015\Projects\Effects11 Samples\C++\DXUT\Optional 
            // However, these types of exceptions can no longer be caught (unless thrown
            // by program code).
            /*
            catch (StackOverflowException e)
            {
                string message = e.Message + "\n   for path " + path + fileName;
                if (IsDebugging)
                    WriteLine(message);
                _errorList.Add(message);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    labelErrorCount.Content = _errorList.Count;
                }));
            }
            */

            // These errors are consumed by the parser, so they can't be caught.
            // This is the reason for catching InvalidOperationException, above.
            /*
            catch (InputMismatchException e)
            {
                string message = e.Message + "\n   for path " + path;
                if (IsDebugging)
                    WriteLine(message);
                _errorList.Add(message);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    labelErrorCount.Content = _errorList.Count;
                }));
            }
            */

            /*
            // For reference.
            public class InputMismatchException extends RecognitionException
            {
                public InputMismatchException(Parser recognizer) {
                    super(recognizer, recognizer.getInputStream(), recognizer._ctx);
                    this.setOffendingToken(recognizer.getCurrentToken());
                }
            }
            */
        }

        /*
        // Might not need this
        public void AddModuleNoDuplicates(string nestedPath, string path, string fileName, string moduleSignature)
        {
            if (!_moduleList.Contains(new ModuleSubgrouped(nestedPath, path, fileName, moduleSignature)) && !_moduleList.Contains(new Module(path, fileName, moduleSignature))
                        && (moduleSignature != null && moduleSignature.Length > 0))
        }
        */

        public void AddModule(string nestedPath, string path, string fileName, string moduleSignature)
        {
            if (_numberOfNestedFolderLevels > 0)
                _moduleList.Add(new ModuleSubgrouped(nestedPath, path, fileName, moduleSignature));
            else
                _moduleList.Add(new Module(path, fileName, moduleSignature));
        }

        // Parse the file with the Roslyn code analyzer.  As this is the same analyzer Microsoft
        // uses for its compiler, it's quite accurate.
        private void ParseRoslyn(string nestedPath, string path, string fileName, string input)
        {
            var tree = CSharpSyntaxTree.ParseText(input);
            //var root = tree.GetRoot();
            // Now not used; replaced with file increment instead.
            //int rowIncrement = 0;

            /*
            // To get the full method bodies:
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach(var method in methods)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listBoxFunctionList.Items.Add(method.ToString());
                }));
            }
            */

            // Create file level row
            //queueList.Add(new Module(path, fileName, "File Level"));
            AddModule(nestedPath, path, fileName, "File Level");
            /*
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //dataGrid.Items.Refresh();
                //rowIncrement++;
                // Don't scroll up at every row addition; otherwise, UI is too unresponsive
                // (especially on these old computers).
                //dataGrid.ScrollIntoView(_moduleList[_moduleList.Count - 1]);
            }));
            */

            // Store the classes in a Dictionary, then check off (set as true) the corresponding members'
            // classes, so that if there are classes with no members, rows can be created for them.
            Dictionary<ClassDeclarationSyntax, bool> classDictionary = new Dictionary<ClassDeclarationSyntax, bool>();
            var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            // There is also EnumDeclarationSyntax
            foreach (var aClass in classes)
            {
                //AddModule(nestedPath, path, fileName, aClass.Identifier.ToFullString().Trim());
                classDictionary[aClass] = false;
            }
            // From http://stackoverflow.com/questions/26436368/how-to-get-method-definition-using-roslyn
            // This line uses LINQ.
            var members = tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>();
            // Cycle through the members, to mark the member's associated classes as having members.
            foreach (var member in members)
            {
                if ((member.Parent as ClassDeclarationSyntax) != null)
                    classDictionary[member.Parent as ClassDeclarationSyntax] = true;
            }
            // Now what will be left are classes with no members in the enclosing scope (not to say though
            // that there aren't members defined elsewhere).  So, produce rows for those "empty" classes.
            // Now cycle through the classDictionary to make rows for those classes who do not have any
            // locally defined members.  Doing it this way (as opposed to producing rows for the "empty"
            // classes after producing rows for the members, marking the member's corresponding classes in
            // the process) requires a second pass, but this program is plenty fast enough, and it shouldn't
            // slow it down much.
            foreach (KeyValuePair<ClassDeclarationSyntax, bool> entry in classDictionary)
            {
                if (!entry.Value)
                    AddModule(nestedPath, path, fileName, entry.Key.Identifier.ToFullString().Trim());
            }

            // Make a second pass through the members, this time producing rows for each property and
            // method signature.
            string oldClassName = null;
            string newClassName;
            foreach (var member in members)
            {
                // The following line is scratch for reference.
                //if (parent.IsKind(SyntaxKind.ClassDeclaration))

                /*
                // Another form of the null conditional operator, the null conditional index operator,
                // which is behaviorally equivalent to: T? item = (collection != null) ? collection[index] : null
                // https://msdn.microsoft.com/en-us/magazine/dn802602.aspx
                foreach (int index in indexes)
                {
                    T? item = collection?[index];
                    if (item != null) yield return (T)item;
                }
                */

                // The following two lines were moved to above, to before the foreach() loop.  Was processed
                // here during the one pass method, but now is processed during the first pass, above.
                //if ((member.Parent as ClassDeclarationSyntax) != null)
                //    classDictionary[member.Parent as ClassDeclarationSyntax] = true;

                // Get the parent class name, and add a row for the class if necessary.
                // This line is from https://johnkoerner.com/page/2/
                // It uses the new null conditional operator, ?.  It returns null if the first operand is null.
                newClassName = (member.Parent as ClassDeclarationSyntax)?.Identifier.ToFullString().Trim();
                //if (newClassName != oldClassName)
                // This is a "quick and dirty" extra check to eliminate duplicates, because for some
                // reason the oldClassName vs. newClassName comparison isn't enough (and sometimes a line 
                // has to be drawn for looking for a fix, in the interest of time).
                // Create new class level row
                if (newClassName != null && !newClassName.Equals(oldClassName))
                {
                    // The following methods of checking for existing rows didn't seem to work
                    // To get the value from the last item (or use _moduleList.Last().FileName):
                    //string lastFileName = _moduleList[_moduleList.Count - 1].FileName;
                    //string lastModuleName = _moduleList[_moduleList.Count - 1].ModuleName;
                    //Module module = new Module(path, fileName, newClassName);
                    //if (!_moduleList.Contains(module))

                    // Search for an already existing row, and don't add if it already exists:
                    // (check for length of newClassName to prevent blank values for enums--not sure if they are encountered
                    // here, or in the block below).
                    // Any() returns true if there is at least one element matching the lambda.
                    //if (!_moduleList.Any(m => m.Path == path && m.FileName == fileName && m.ModuleName == newClassName) 
                    //if (!_moduleList.Exists(x => x.Path == path && x.FileName == fileName && x.ModuleName == newClassName)
                    //            && (newClassName != null && newClassName.Length > 0))
                    // When I changed ModuleList from a List<T> to an ObservableCollection<T> I had to modify the line above to:
                    if (!_moduleList.Contains(new ModuleSubgrouped(nestedPath, path, fileName, newClassName)) && !_moduleList.Contains(new Module(path, fileName, newClassName))
                        && (newClassName != null && newClassName.Length > 0))
                    {

                        //queueList.Add(new Module(path, fileName, newClassName));
                        AddModule(nestedPath, path, fileName, newClassName);
                        /*
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _moduleList.Add(new Module(path, fileName, newClassName));
                            //rowIncrement++;
                            // Only autoscroll part of the time; otherwise, the UI is too unresponsive.
                            //if (rowIncrement % refreshCount == 0)
                            //{
                            //    dataGrid.Items.Refresh();
                            //    dataGrid.ScrollIntoView(_moduleList[_moduleList.Count - 1]);
                            //}
                        }));
                        */
                    }
                    oldClassName = newClassName;
                }

                // Add a row for the property.
                var property = member as PropertyDeclarationSyntax;
                // Check for Length > 0 to prevent populating blank module names (not sure if they are encountered here,
                // or in the block above).
                if (property != null && property.Identifier.ToString().Length > 0)
                {
                    //listBoxFunctionList.Items.Add("Property: " + property.Identifier);
                    string classOrInterfaceName = (property.Parent as ClassDeclarationSyntax)?.Identifier.ToFullString().Trim();
                    // If it is not a class name, it is probably an interface instead.
                    if (classOrInterfaceName == null)
                        // Don't add rows for interfaces, so we can do nothing here instead.
                        classOrInterfaceName = (property.Parent as InterfaceDeclarationSyntax)?.Identifier.ToFullString().Trim();
                    else
                    {
                        //queueList.Add(new Module(path, fileName, classOrInterfaceName + "." + property.Identifier));
                        AddModule(nestedPath, path, fileName, classOrInterfaceName + "." + property.Identifier);
                        /*
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _moduleList.Add(new Module(path, fileName, classOrInterfaceName + "." + property.Identifier));
                            // Is not an interface, so add a row for it.
                            //rowIncrement++;
                            // Only autoscroll part of the time; otherwise, the UI is too unresponsive.
                            //if (rowIncrement % refreshCount == 0)
                            //{
                            //    dataGrid.Items.Refresh();
                            //    dataGrid.ScrollIntoView(_moduleList[_moduleList.Count - 1]);
                            //}

                        }));
                        */
                    }
                }

                // some of the classes from Microsoft.CodeAnalysis.CSharp/Syntax:
                // AnonymousMethodExpressionSyntax
                // DeclarationStatementSyntax
                // DelegateDeclarationSyntax
                // NamespaceDeclarationSyntax
                // TypeDeclarationSyntax
                // ConstructorDeclarationSyntax

                // Add rows for constructors.
                BaseMethodDeclarationSyntax method = member as ConstructorDeclarationSyntax;
                // and add rows for methods.
                if (method == null)
                    method = member as MethodDeclarationSyntax;
                if (method != null)
                {
                    //_moduleList.Add(new Module(path, fileName, method.Identifier + "" + method.ParameterList));
                    StringBuilder typeList = new StringBuilder();
                    typeList.Append("(");

                    // For reference see this page:  http://www.codeproject.com/Articles/606931/Getting-Started-with-Roslyn-Part
                    SeparatedSyntaxList<ParameterSyntax> paramList = method.ParameterList.Parameters;
                    foreach (ParameterSyntax arg in paramList)
                    {
                        if (typeList.Length > 1)
                            typeList.Append(", ");
                        // Show type only
                        typeList.Append(arg.Type);
                        //typeList.Append(arg.Identifier);
                    }
                    typeList.Append(")");
                    string classOrInterfaceName = (method.Parent as ClassDeclarationSyntax)?.Identifier.ToFullString().Trim();
                    // If it is not a class name, try interface instead.
                    if (classOrInterfaceName == null)
                        // Don't add rows for interfaces, so we could do nothing here instead.
                        classOrInterfaceName = (method.Parent as InterfaceDeclarationSyntax)?.Identifier.ToFullString().Trim();
                    else
                    {
                        //_moduleList.Add(new Module(path, fileName, method.Identifier + "" + method.TypeParameterList));
                        //_moduleList.Add(new Module(path, fileName, method.Identifier + "" + method.ParameterList));
                        string signature = "";
                        if (method is ConstructorDeclarationSyntax)
                            signature = classOrInterfaceName + "." + ((ConstructorDeclarationSyntax)method).Identifier + typeList;
                        else //if (method is MethodDeclarationSyntax)  <--always true at this point
                            signature = classOrInterfaceName + "." + ((MethodDeclarationSyntax)method).Identifier + typeList;

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // Is not an interface, so add a row for it.
                            //_moduleList.Add(new Module(path, fileName, signature));
                            listBoxFunctionList.Items.Add(signature);

                            //rowIncrement++;
                            // Only autoscroll part of the time; otherwise, the UI is too unresponsive.
                            //if (rowIncrement % refreshCount == 0)
                            //{
                            //    dataGrid.Items.Refresh();
                            //    dataGrid.ScrollIntoView(_moduleList[_moduleList.Count - 1]);
                            //}
                            //listBoxFunctionList.Items.Add("Method: " + method.Identifier);
                            //method.WithParameterList()
                            //if (method is ConstructorDeclarationSyntax)
                            //   listBoxFunctionList.Items.Add(((ConstructorDeclarationSyntax)method).Identifier + method.TypeParameterList + "" + method.ParameterList);
                            //else if (method is MethodDeclarationSyntax)
                            //    listBoxFunctionList.Items.Add(((MethodDeclarationSyntax)method).Identifier + method.TypeParameterList + "" + method.ParameterList);
                        }));
                        //queueList.Add(new Module(path, fileName, signature));
                        AddModule(nestedPath, path, fileName, signature);

                    }
                }
            }

            /*
            // http://www.filipekberg.se/2011/10/21/getting-all-methods-from-a-code-file-with-roslyn/
            However, you might be a bit confused as to how you print the method name, because there's not a Name-property on the object! Instead there is something called an Identifier that we can use:
            foreach (var method in methods)
            {
                Console.WriteLine(method.Identifier);
            }
            This will print all methods and not including the constructors, if we want to get the constructors we ask for ConstructorDeclarationSyntax instead of MethodDeclarationSyntax.We can get a lot of interesting things from the method-object in the iterator, we can ask about the parameters, the return type and a lot of other nice things.
           - See more at: http://www.filipekberg.se/2011/10/21/getting-all-methods-from-a-code-file-with-roslyn/#sthash.nhtVejaU.dpuf
           */

            /*
            // Scroll to the last item at the end of processing.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                dataGrid.Items.Refresh();
                try
                {
                    dataGrid.ScrollIntoView(_moduleList[_moduleList.Count - 1]);
                }
                catch (ArgumentOutOfRangeException e)
                { }
                dataGrid.Items.Refresh();
            }));
            */

            PeriodicProcess();

        }

        // Parsing using regular expressions is not as accurate as using a full fledged code parser, but can still be useful.
        private void ParseRegex(string input)
        {
            string pattern = @"^[^\S\r\n]*(?&lt;modifier1&gt;(?:public|protected|internal|private)\s*)?(?&lt;modifier2&gt;(?:new|static|virtual|sealed|override|abstract|extern)\s*)?(partial\s*)?(?&lt;type&gt;(?!(return|if|else))\w+(?&lt;genericType&gt;&lt;[\w,\s&lt;&gt;]+&gt;)?\s+)(?&lt;name&gt;\w+(?&lt;genericNameType&gt;&lt;[\w,\s&lt;&gt;]+&gt;)?\s?)\((?&lt;params&gt;[\w\s,&lt;&gt;\[\]\:=\.]*)\)(?&lt;ctorChain&gt;\s*\:\s*(?:base|this)\s*\((?&lt;ctorParams&gt;[\w\s,&lt;&gt;\[\]\:=\.]*)\))?[\w\s&lt;&gt;\:,\(\)\[\]]*(?:\{|;)";

            //string pattern = @"^[^\S\r\n]*(?&lt;modifier1&gt;(?:public|protected|internal|private)\s*)?(?&lt;modifier2&gt;(?:new|static|virtual|sealed|override|abstract|extern)\s*)?(partial\s*)?(?&lt;type&gt;(?!(return|if|else))\w+(?&lt;genericType&gt;&lt;[\w,\s&lt;&gt;]+&gt;)?\s+)(?&lt;name&gt;\w+(?&lt;genericNameType&gt;&lt;[\w,\s&lt;&gt;]+&gt;)?\s?)\((?&lt;params&gt;[\w\s,&lt;&gt;\[\]\:=\.]*)\)(?&lt;ctorChain&gt;\s*\:\s*(?:base|this)\s*\((?&lt;ctorParams&gt;[\w\s,&lt;&gt;\[\]\:=\.]*)\))?[\w\s&lt;&gt;\:,\(\)\[\]]*(?:\{|;)";

            /*
            string pattern = @"^[ \t]*[a-z_.0-9]+[ \t]+[a-z_.0-9]+[ \t]+[a-z_.0-9<>]+[ \t]+([0-9\\.a-z_ \t]*)\(+[^=;\n]+$";
            Regex regex = new Regex(pattern);
            stringContents = File.ReadAllText(path);
            foreach (Match match in regex.Matches(stringContents))
                listBoxFunctionList.Items.Add(match.Value);
            pattern = @"^[ \t]*[a-z_\.0-9]+[ \t]+[a-z_\.0-9<>]+[ \t]+([0-9a-z_\. \t]*)\(+[^=;\n]+$";
            regex = new Regex(pattern);
            foreach (Match match in regex.Matches(stringContents))
                listBoxFunctionList.Items.Add(match.Value);
            pattern = @"[ ^t]*p+[a-z_.0-9]+[ \t]+([0-9a-z_. \t]*)\(+[^=;\n]+$";
            regex = new Regex(pattern);
            foreach (Match match in regex.Matches(stringContents))
                listBoxFunctionList.Items.Add(match.Value);
            pattern = @"^[ \t]*[a-z_0-9]+[ \t]+([0-9a-z_]+\.[0-9a-z_]+)\(+[^=;\n]+$";
            regex = new Regex(pattern);
            foreach (Match match in regex.Matches(stringContents))
                listBoxFunctionList.Items.Add(match.Value);
            */

            //var regex = new PcreRegex(regexCSharpFunction);
            ////System.Collections.Generic.IEnumerable<PcreMatch> matches = regex.Matches(stringContents);
            //var matches = regex.Matches(stringContents);
            //if (matches.Any())
            //    foreach (var match in matches)
            //        listBoxFunctionList.Items.Add(match.Value);

            //Regex regex = new Regex(regexCSharpFunction);
            // perl style: @"^[ \t]*[a-z_\.0-9]+[ \t]+[a-z_\.0-9]+[ \t]+[a-z_\.0-9<>]+[ \t]+([0-9\\.a-z_ \t]*)\(+[^=;\n]+$"
            //Regex regex = new Regex(@"^[ \t]*[a-z_.0-9]+[ \t]+[a-z_.0-9]+[ \t]+[a-z_.0-9<>]+[ \t]+([0-9\\.a-z_ \t]*)\(+[^=;\n]+$");

            // The following is the Perl pattern converted to a C# pattern
            //Regex regex = new Regex(@"^[^\\S\\r\\n]*(?<modifier1>(?:public|protected|internal|private)\\s*)?(?<modifier2>(?:new|static|virtual|sealed|override|abstract|extern)\\s*)?(partial\\s*)?(?<type>(?!(return|if|else))\\w+(?<genericType><[\\w,\\s<>]+>)?\\s+)(?<name>\\w+(?<genericNameType><[\\w,\\s<>]+>)?\\s?)\\((?<params>[\\w\\s,<>\\[\\]\\:=\\.\]*)\\)(?<ctorChain>\\s*\\:\\s*(?:base|this)\\s*\\((?<ctorParams>[\\w\\s,<>\\[\\]\\:=\\.\]*)\\))?[\\w\\s<>\\:,\\(\\)\\[\\]\]*(?:\\{|;)");

            // The following pattern is from this page: http://stackoverflow.com/questions/2913158/find-methods-in-a-c-sharp-file-programmatically
            // (So far, the only one for C# that seems to work!)
            Regex regex = new Regex(@"(?<=^|;|\{|\})[^;{}]*\s([\w.]+)\s*\([^;{}]*");

            // The following pattern is a RegexBuddy conversion of the first Functions Perl regex to C# from csharp.uew from Ultra Edit
            //Regex regex = new Regex(@"^[ \t]*[a-z_.0-9]+[ \t]+[a-z_.0-9]+[ \t]+[a-z_.0-9<>]+[ \t]+([0-9\\.a-z_ \t]*)\(+[^=;\n]+$");

            // The following pattern is a RegexBuddy conversion of the second Functions Perl regex to C# from csharp.uew from Ultra Edit
            //Regex regex = new Regex(@"^[ \t]*[a-z_.0-9]+[ \t]+[a-z_.0-9<>]+[ \t]+([0-9a-z_. \t]*)\(+[^=;\n]+$");

            // The following pattern is a RegexBuddy conversion of the third Functions Perl regex to C# from csharp.uew from Ultra Edit
            //Regex regex = new Regex(@"^[ ^t]*p+[a-z_.0-9]+[ \t]+([0-9a-z_. \t]*)\(+[^=;\n]+$");

            // The following pattern is a RegexBuddy conversion of the third Functions Perl regex to C# from csharp.uew from Ultra Edit
            //Regex regex = new Regex(@"^[ \t]*[a-z_0-9]+[ \t]+([0-9a-z_]+\.[0-9a-z_]+)\(+[^=;\n]+$");


            MatchCollection matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                foreach (Group group in match.Groups)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        listBoxFunctionList.Items.Add(group.Value);
                    }));
                }
            }

            /*
            string pattern2 = "class";
            var regex2 = new PcreRegex(pattern2);
            var matches2 = regex2.Matches(stringContents);
            //foreach (var match2 in matches)
            //    listBoxFunctionList.Items.Add(match2.Value);
            foreach (Match m1 in matches2)
                foreach (Group g in m1.Groups)
                    listBoxFunctionList.Items.Add(g.Value);
            */

            //WriteLine("Processed file '{0}'.", path);
        }


    }
}
