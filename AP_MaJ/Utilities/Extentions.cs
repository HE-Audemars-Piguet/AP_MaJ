using Autodesk.Connectivity.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    internal static class ExtensionMethods
    {
        internal static ByteArray ToByteArray(this byte[] inputArray)
        {
            return new ByteArray() { Bytes = inputArray };
        }
    }
}
