using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    public static class EncodingUtility
    {
        public static Encoding DetectFileEncoding(string strFilePath, Encoding fallBackEncoding)
        {
            Encoding detectedEncoding = fallBackEncoding;

            using (FileStream fs = System.IO.File.OpenRead(strFilePath))
            {
                Ude.CharsetDetector cdet = new Ude.CharsetDetector();
                cdet.Feed(fs);
                cdet.DataEnd();

                if (cdet.Charset != null)
                {
                    //MessageBox.Show(string.Format("Charset: {0}, confidence: {1}", cdet.Charset, cdet.Confidence));
                    detectedEncoding = Encoding.GetEncoding(cdet.Charset);
                }
                else
                {
                    System.Windows.MessageBox.Show("Unable to detect file '" + strFilePath + "' encoding." + Environment.NewLine + fallBackEncoding.EncodingName + " will be used.");
                }
            }

            return detectedEncoding;
        }
    }
}
