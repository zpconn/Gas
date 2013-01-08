using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;

namespace Gas.Helpers
{
    /// <summary>
    /// This class will automatically create a log file. It provides static
    /// functionality for writing log/error/debug information for run-time error
    /// checking.
    /// </summary>
    public class Log
    {
        #region Variables
        private static StreamWriter writer = null;
        #endregion

        #region Static constructor to create log file
        static Log()
        {
            try
            {
                FileStream file = new FileStream( "Log.txt",
                    FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );

                // If the log file is larger than 2MB, destory it and create a new one
                if ( file.Length > 2 * 1024 * 1024 )
                {
                    file.Close();
                    file = new FileStream( "Log.txt",
                        FileMode.Create, FileAccess.Write, FileShare.ReadWrite );
                }

                // Ensure that the UTF-8 sign is written
                if ( file.Length == 0 )
                    writer = new StreamWriter( file, System.Text.Encoding.UTF8 );
                else
                    writer = new StreamWriter( file );

                writer.BaseStream.Seek( 0, SeekOrigin.End );
                writer.AutoFlush = true;

                // Add some information about this session
                writer.WriteLine( "" );
                writer.WriteLine( "/// Session started at: " +
                    DateTime.Now.ToString() );
                writer.WriteLine( "/// " + Application.ProductName +
                    " v" + Application.ProductVersion );
                writer.WriteLine( "" );
            }
            catch
            {
                // Just ignore the exception
            }
        }
        #endregion

        #region Write log entry
        static public void Write( string message )
        {
            if ( writer == null )
                return;

            try
            {
                DateTime dt = DateTime.Now;
                string s = "[" + dt.Hour.ToString( "00" ) + ":" +
                    dt.Minute.ToString( "00" ) + ":" +
                    dt.Second.ToString( "00" ) + "] " +
                    message;
                writer.WriteLine( s );
            }
            catch
            {
                // Just ignore the exception
            }
        }
        #endregion
    }
}