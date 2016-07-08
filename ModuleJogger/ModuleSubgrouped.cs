namespace ModuleJogger
{
    /// <summary>
    /// This class is a subclass of Module, and adds one more field containing a path 
    /// prefix used for sub-grouping, used for a fourth column in the module results.
    /// </summary>
    class ModuleSubgrouped : Module
    {
        private string nestedPath;
        public string NestedPath
        {
            get { return nestedPath; }
            set
            {
                if (nestedPath != value)
                {
                    nestedPath = value;
                    NotifyPropertyChanged("NestedPath");
                }
            }
        }

        public ModuleSubgrouped(string nestedPath, string path, string fileName, string moduleName) : base(path, fileName, moduleName)
        {
            NestedPath = nestedPath;
        }

    }
}
