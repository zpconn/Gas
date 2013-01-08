using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gas.Helpers
{
    /// <summary>
    /// Provides static access to native Win32 methods.
    /// </summary>
    internal static class NativeMethods
    {
        #region Win32 Messages
        /// <summary>
        /// Stores a Win32 message.
        /// </summary>
        [StructLayout( LayoutKind.Sequential )]
        public struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        /// <summary>
        /// Tells us whether or not there's a new Win32 message to be handled.
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport( "User32.dll", CharSet = CharSet.Auto )]
        public static extern bool PeekMessage( out Message msg, IntPtr hWnd,
            uint messageFilterMin, uint messageFilterMax, uint flags );
        #endregion

        #region Query performance frequency/counter
        /// <summary>
        /// Queries the performance frequency for high-resolution timing. The frequency
        /// is the number of ticks per second.
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport( "Kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool QueryPerformanceFrequency( out long lpFrequency );

        /// <summary>
        /// Queries the performance counter for high-resolution timing.
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport( "Kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool QueryPerformanceCounter( out long lpCounter );

        /// <summary>
        /// Gets the performance frequency.
        /// </summary>
        public static long GetPerformanceFrequency()
        {
            long freq;
            QueryPerformanceFrequency( out freq );
            return freq;
        }

        /// <summary>
        /// Gets the performance counter.
        /// </summary>
        public static long GetPerformanceCounter()
        {
            long counter;
            QueryPerformanceCounter( out counter );
            return counter;
        }
        #endregion
    }
}