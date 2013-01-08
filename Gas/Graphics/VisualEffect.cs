using System;
using System.Collections.Generic;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// This attribute indicates that the class it's attached to is the main class in the VisualEffect DLL file.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class MainVisualEffectClassAttribute : Attribute
    {
    }

    /// <summary>
    /// This base class defines the general interface which all visual effects must implement in their
    /// associated DLL files. The implementation of this class in the DLL should be flagged with the
    /// MainVisualEffectClass attribute.
    /// </summary>
    public class VisualEffect : IDisposable
    {
        #region Variables
        protected Renderer renderer = null;
        #endregion

        #region Public interface
        /// <summary>
        /// This is called to prepare the VisualEffect for rendering the scene.
        /// </summary>
        public virtual void BeginRenderScene() { }

        /// <summary>
        /// This is called to inform the VisualEffect that scene rendering is over.
        /// </summary>
        public virtual void EndRenderScene() { }

        /// <summary>
        /// This is called right before rendering a specific geometry chunk.
        /// </summary>
        public virtual void BeginRenderObject( Material material ) { }

        /// <summary>
        /// This is called right before rendering a specific geometry chunk. Includes a parameter
        /// for passing extra data needed for rendering this specific chunk.
        /// </summary>
        public virtual void BeginRenderObject( Material material, List<object> extraData ) { }

        /// <summary>
        /// This is called right after rendering a specific geometry chunk.
        /// </summary>
        public virtual void EndRenderObject() { }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of VisualEffect.
        /// </summary>
        /// <param name="renderer"></param>
        public VisualEffect( Renderer renderer )
        {
            if ( renderer == null )
            {
                Log.Write( "Can't create a VisualEffect with a null Renderer reference." );
                throw new ArgumentNullException( "renderer",
                    "Can't create a VisualEffect with a null Renderer reference." );
            }

            this.renderer = renderer;
        }
        #endregion

        #region Disposing
        public virtual void Dispose()
        {
        }
        #endregion
    }
}