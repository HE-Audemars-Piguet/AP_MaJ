using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    public class TaskProgressReport
    {
        public int TotalEntityCount { get; set; } = -1;
        public string Message { get; set; } = string.Empty;
        public string Timer { get; set; } = "00:00";
    }
    public class ProcessProgressReport
    {
        public int ProcessIndex { get; set; }
        public string Message { get; set; } = string.Empty;

        public int TotalCountInc { get; set; } = 0;
        public int DoneInc { get; set; } = 0;
        public int ErrorInc { get; set; } = 0;
        //public bool? ProcessHasError { get; set; } = null;
    }
}
