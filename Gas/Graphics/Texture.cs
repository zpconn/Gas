using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// Encapsulates a Direct3D.Texture object. Also provides functionality for setting the texture
    /// as a render target.
    /// </summary>
    public class Texture : IGraphicsResource
    {
        #region Variables
        Renderer renderer = null;

        private string texFilename = "";
        private Size size;

        private Direct3D.Texture d3dTexture = null;

        private bool loaded = false;
        private bool hasAlpha = false;

        /// <summary>
        /// Used for rendering to a texture.
        /// </summary>
        private Direct3D.Surface d3dSurface = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the texture filename.
        /// </summary>
        public string Filename
        {
            get
            {
                return texFilename;
            }
        }

        /// <summary>
        /// Gets the size of the texture.
        /// </summary>
        public Size Size
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// Gets the width of the texture.
        /// </summary>
        public int Width
        {
            get
            {
                return size.Width;
            }
        }

        /// <summary>
        /// Gets the height of the texture.
        /// </summary>
        public int Height
        {
            get
            {
                return size.Height;
            }
        }

        /// <summary>
        /// Gets the Direct3D texture object.
        /// </summary>
        public Direct3D.Texture D3DTexture
        {
            get
            {
                return d3dTexture;
            }
        }

        /// <summary>
        /// Is this texture valid to use?
        /// </summary>
        public bool Valid
        {
            get
            {
                return loaded && ( d3dTexture != null );
            }
        }

        /// <summary>
        /// Does this texture contain alpha information?
        /// </summary>
        public bool HasAlphaPixels
        {
            get
            {
                return hasAlpha;
            }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Creates the texture from a file.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="filename">The name of the texture file to load.</param>
        public Texture( Renderer renderer, string filename )
        {
            if ( renderer == null )
                throw new ArgumentNullException( "renderer",
                    "Unable to create texture without a valid renderer reference." );

            if ( String.IsNullOrEmpty( filename ) )
                throw new ArgumentNullException( "filename",
                    "Unable to create texture without valid filename." );

            this.renderer = renderer;
            texFilename = filename;

            // Try to load the texture
            try
            {
                if ( File.Exists( filename ) == false )
                    throw new FileNotFoundException( filename );

                ImageInformation imageInfo = TextureLoader.ImageInformationFromFile( filename );
                size = new Size( imageInfo.Width, imageInfo.Height );

                if ( size.Width == 0 || size.Height == 0 )
                    throw new InvalidOperationException(
                        "Image size=" + size + " is invalid, unable to create texture." );

                hasAlpha = imageInfo.Format == Format.Dxt5 ||
                    imageInfo.Format == Format.Dxt3 ||
                    imageInfo.Format.ToString().StartsWith( "A" );

                d3dTexture = TextureLoader.FromFile( this.renderer.Device, filename );

                loaded = true;

                renderer.AddGraphicsObject( this );
            }
            catch ( Exception ex )
            {
                loaded = false;
                Log.Write( "Failed to load texture " + filename +
                    ", will use empty texture! Error: " + ex.ToString() );
            }
        }

        /// <summary>
        /// Creates the texture as a render target.
        /// </summary>
        public Texture( Renderer renderer, int width, int height, bool alpha )
        {
            if ( renderer == null )
                throw new ArgumentNullException( "renderer",
                    "Unable to create texture without a valid renderer reference." );

            this.renderer = renderer;

            try
            {
                d3dTexture = new Microsoft.DirectX.Direct3D.Texture( renderer.Device,
                    width, height, 1, Usage.RenderTarget, alpha ? Format.A8R8G8B8 : Format.X8R8G8B8,
                    Pool.Default );

                d3dSurface = d3dTexture.GetSurfaceLevel( 0 );

                this.size = new Size( width, height );
                this.hasAlpha = alpha;

                loaded = true;

                renderer.AddGraphicsObject( this );
            }
            catch ( Exception ex )
            {
                loaded = false;
                Log.Write( "Failed to create texture as render target, will use empty texture!" +
                    " Error: " + ex.ToString() );
            }
        }
        #endregion

        #region Disposing
        public virtual void Dispose()
        {
            if ( d3dTexture != null )
                d3dTexture.Dispose();
            d3dTexture = null;

            if ( d3dSurface != null )
                d3dSurface.Dispose();
            d3dSurface = null;

            loaded = false;
        }
        #endregion

        #region IGraphicsObject members
        /// <summary>
        /// Handles a device reset.
        /// </summary>
        public virtual void OnDeviceReset()
        {
        }

        /// <summary>
        /// Handles a lost device.
        /// </summary>
        public virtual void OnDeviceLost()
        {
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Places this texture on texture stage 0.
        /// </summary>
        public void Select()
        {
            if ( d3dTexture != null )
                renderer.Device.SetTexture( 0, d3dTexture );
        }

        /// <summary>
        /// Sets this texture as the render target.
        /// </summary>
        public void SetAsRenderTarget()
        {
            renderer.Device.SetRenderTarget( 0, d3dSurface );
        }
        #endregion
    }
}