using System.Runtime.CompilerServices;

namespace ModuleJogger
{
    /// <summary>
    /// This is a class that simply contains a file count.  At this point
    /// it's probably unnecessary, but was used previously to pass into the Task
    /// method to hold a value.
    /// </summary>
    public class NumberOfFiles
    {
        //public int Count { get; set; }
        //public void Increment()
        //{
        //    Count++;
        //}

        private int count;
        public int Count
        {
            get
            { return count; }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            { count = value; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Increment()
        {
            count++;
        }
    }

}


