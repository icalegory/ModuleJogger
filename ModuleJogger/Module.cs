using System.ComponentModel;

namespace ModuleJogger
{
    /// <summary>
    /// This is an object the represents a row in the DataGrid.
    /// </summary>
    class Module : INotifyPropertyChanged
    {
        //public string Path { get; set; }
        //public string FileName { get; set; }
        //public string ModuleName { get; set; }

        // If it is necessary to NotifyPropertyChanged(), use the following
        // from http://www.wpf-tutorial.com/data-binding/responding-to-changes/

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                if (path != value)
                {
                    path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        public string moduleName;
        public string ModuleName
        {
            get { return moduleName; }
            set
            {
                if (moduleName != value)
                {
                    moduleName = value;
                    NotifyPropertyChanged("ModuleName");
                }
            }
        }

        public Module(string path, string fileName, string moduleName)
        {
            Path = path;
            FileName = fileName;
            ModuleName = moduleName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
