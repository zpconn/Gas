using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace Gas.Helpers
{
    /// <summary>
    /// High-resolution timer. Adapted from the Rocket Commander source freely available
    /// from http://www.rocketcommander.com -- I also cleaned up the code quite a bit, and
    /// made the comments a tad more lucid. ^^
    /// </summary>
    public class HighResTimer
    {
        #region Variables
        /// <summary>
        /// The time (in nanoseconds) that this HighResTimer started timing.
        /// </summary>
        private long startTimeNs = ConvertToNs( NativeMethods.GetPerformanceCounter() );

        /// <summary>
        /// The time (in nanoseconds) that the last frame ended.
        /// </summary>
        private long lastTimeNs = 0;

        /// <summary>
        /// The total time (in nanoseconds) elapsed since this HighResTimer started timing.
        /// </summary>
        private long elapsedTimeNs = 0;

        /// <summary>
        /// The length of the last frame in nanoseconds.
        /// </summary>
        private long elapsedTimeLastFrameNs = 1000;

        /// <summary>
        /// The length of the last frame in milliseconds.
        /// </summary>
        private long elapsedTimeLastFrameMs = 1;

        /// <summary>
        /// The number of frames passed this second.
        /// </summary>
        private long frameCountThisSecond = 0;

        /// <summary>
        /// The total number of frames passed since this HighResTimer started timing.
        /// </summary>
        private int totalFrameCount = 0;

        /// <summary>
        /// The FPS last second.
        /// </summary>
        private int fpsLastSecond = 1;

        /// <summary>
        /// The time (in nanoseconds) that this second started.
        /// </summary>
        private long startTimeThisSecondNs = 0;

        /// <summary>
        /// The performance frequency.
        /// </summary>
        private static long performanceFrequency = NativeMethods.GetPerformanceFrequency();
        #endregion

        #region Properties
        /// <summary>
        /// The time elapsed last frame in nanoseconds.
        /// </summary>
        public int LastFrameElapsedNs
        {
            get
            {
                return ( int )elapsedTimeLastFrameNs;
            }
        }

        /// <summary>
        /// The total time elapsed (in nanoseconds) since this HighResTimer started timing.
        /// </summary>
        public int TotalElapsedNs
        {
            get
            {
                return ( int )elapsedTimeNs;
            }
        }

        /// <summary>
        /// The time elapsed last frame in milliseconds.
        /// </summary>
        public int LastFrameElapsedMs
        {
            get
            {
                return ( int )elapsedTimeLastFrameMs;
            }
        }

        /// <summary>
        /// The total time elapsed (in milliseconds) since this HighResTimer started timing.
        /// </summary>
        public int TotalElapsedMs
        {
            get
            {
                return ( int )( elapsedTimeNs / 1000 );
            }
        }

        /// <summary>
        /// The frames per second. Updated once per second.
        /// </summary>
        public int FramesPerSecond
        {
            get
            {
                if ( fpsLastSecond <= 0 )
                    return 1;

                return fpsLastSecond;
            }
        }

        /// <summary>
        /// The total number of frames elapsed since this HighResTimer started timing.
        /// </summary>
        public int TotalFrames
        {
            get
            {
                return totalFrameCount;
            }
        }

        /// <summary>
        /// This is the factor by which physics calculations should be scaled. This
        /// helps to ensure that motion proceeds in a time-based manner rather than
        /// a frame-based one. It also allows motion constants to be expressed
        /// in terms of a rigid time unit (the second).
        /// 
        /// As an example, if we are getting 1 frame per second, this will be 1.0f. If
        /// we are getting 100 frames per second, then this will be 0.01f.
        /// </summary>
        public float MoveFactorPerSecond
        {
            get
            {
                return elapsedTimeLastFrameNs / 1000000.0f;
            }
        }
        #endregion

        #region Timing methods
        /// <summary>
        /// Converts a performance counter value to nanoseconds.
        /// </summary>
        public static long ConvertToNs( long perfCounter )
        {
            return perfCounter * 1000000 / performanceFrequency;
        }

        /// <summary>
        /// Updates the timing statistics.
        /// </summary>
        public void Update()
        {
            long currentTimeNs = ConvertToNs( NativeMethods.GetPerformanceCounter() );

            if ( lastTimeNs == 0 )
            {
                lastTimeNs = startTimeNs;

                if ( currentTimeNs - lastTimeNs > 100000 )
                    lastTimeNs = currentTimeNs - 100000;
            }

            elapsedTimeLastFrameNs = ( int )( currentTimeNs - lastTimeNs );
            elapsedTimeLastFrameMs = ( int )( ( ( elapsedTimeNs + elapsedTimeLastFrameNs ) / 1000 ) -
                ( elapsedTimeNs / 1000 ) );

            lastTimeNs += elapsedTimeLastFrameNs;
            elapsedTimeNs += elapsedTimeLastFrameNs;

            frameCountThisSecond++;
            totalFrameCount++;

            // Has a single second elapsed?
            if ( Math.Abs( ( currentTimeNs - startTimeThisSecondNs ) ) > 1000000 )
            {
                // Calculate the FPS
                fpsLastSecond = ( int )( ( float )( frameCountThisSecond * 1000000 ) /
                    ( currentTimeNs - startTimeThisSecondNs ) );
                startTimeThisSecondNs = currentTimeNs;
                frameCountThisSecond = 0;
            }
        }
        #endregion
    }
}
