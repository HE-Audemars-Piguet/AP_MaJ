using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP_MaJ.Utilities
{
    public class TaskProgressReport
    {
        public int TotalEntityCount { get; set; }
        public string Message { get; set; }
        public string Timer { get; set; }
    }
    public class ProcessProgressReport
    {
        public int ProcessIndex { get; set; }
        public string ProcessFeedbackMessage { get; set; }
        public bool? ProcessHasError { get; set; } = null;
    }
}
