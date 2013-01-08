using System;

namespace Gas.Graphics
{
    /// <summary>
    /// Defines the public interface to be implemented by all graphics objects that provide
    /// rendering functionality.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Renders the object.
        /// </summary>
        void Render();
    }
}