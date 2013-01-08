using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.DirectX;
using Gas.Graphics;

namespace Gas.Graphics
{
    /// <summary>
    /// Represents a flat, textured 2D surface which is rendered with a Material.
    /// </summary>
    public class Surface : IRenderable
    {
        #region Variables
        private string materialName = null;
        private Mesh quad = null;
        private Size size = new Size();
        private Renderer renderer = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of the Surface in pixels.
        /// </summary>
        public Size Size
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// Gets the Mesh used for rendering the surface.
        /// </summary>
        public Mesh Mesh
        {
            get
            {
                return quad;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of Surface.
        /// </summary>
        public Surface( Renderer renderer, string materialName, Size size )
        {
            this.renderer = renderer;
            this.materialName = materialName;
            this.size = size;

            quad = Mesh.Rectangle( renderer, Color.White, size.Width, size.Height );
        }

        /// <summary>
        /// Initializes a new instance of Surface.
        /// </summary>
        public Surface( Renderer renderer, string materialName, Size size,
            float textureMapTiling )
        {
            this.renderer = renderer;
            this.materialName = materialName;
            this.size = size;

            quad = Mesh.Rectangle( renderer, Color.Black, size.Width, size.Height, textureMapTiling );
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the surface.
        /// </summary>
        public void Render( Matrix worldTransform )
        {
            renderer.AddRenderPacket( new RenderPacket( quad, materialName, worldTransform ) );
        }

        /// <summary>
        /// Renders the surface.
        /// </summary>
        /// <param name="pos">The position of the upper-left corner in world space.</param>
        public void Render( Vector3 pos )
        {
            Render( Matrix.Translation( pos ) );
        }

        /// <summary>
        /// Renders the surface.
        /// </summary>
        public void Render( Matrix worldTransform, List<object> extraData )
        {
            renderer.AddRenderPacket( new RenderPacket( quad, materialName, worldTransform, extraData ) );
        }

        /// <summary>
        /// Renders the surface.
        /// </summary>
        /// <param name="pos">The position of the upper-left corner in world space.</param>
        public void Render( Vector3 pos, List<object> extraData )
        {
            Render( Matrix.Translation( pos ), extraData );
        }

        /// <summary>
        /// Renders the surface at (0,0,0).
        /// </summary>
        public void Render()
        {
            Render( Matrix.Identity );
        }
        #endregion
    }
}