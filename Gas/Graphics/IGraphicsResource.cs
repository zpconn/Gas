using System;
using System.Collections.Generic;
using System.Text;

namespace Gas.Graphics
{
    /// <summary>
    /// Base interface for all graphics objects. The renderer maintains a list of 
    /// IGraphicsObjects and updates them when the Direct3D device is reset or lost.
    /// </summary>
    public interface IGraphicsResource : IDisposable
    {
        /// <summary>
        /// Called when the Direct3D device is reset.
        /// </summary>
        void OnDeviceReset();

        /// <summary>
        /// Called when the Direct3D device is lost.
        /// </summary>
        void OnDeviceLost();
    }
}
