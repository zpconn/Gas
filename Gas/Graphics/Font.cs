using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// Encapsulates a Direct3D font object. Automatically handles dealing with
    /// Direct3D.Sprite, and provides auxiliary methods for specialized text
    /// rendering.
    /// </summary>
    public class Font : IGraphicsResource
    {
        #region Variables
        Renderer renderer = null;
        System.Drawing.Font windowsFont = null;
        Direct3D.Font d3dFont = null;
        Direct3D.Sprite textSprite = null;
        string familyName = "";
        int height = 10;

        private Color shadowColor = Color.Black;
        private int shadowOffset = 3;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the shadow color.
        /// </summary>
        public Color ShadowColor
        {
            get
            {
                return shadowColor;
            }

            set
            {
                shadowColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the shadow offset.
        /// </summary>
        public int ShadowOffset
        {
            get
            {
                return shadowOffset;
            }

            set
            {
                shadowOffset = value;
            }
        }

        /// <summary>
        /// Gets the height of the font.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the font object using the desired font family. If the desired family
        /// is not supported, the fallback family defined in the Settings file is used.
        /// </summary>
        public Font( Renderer renderer, string familyName, int height )
        {
            this.renderer = renderer;
            this.familyName = familyName;
            this.height = height + 5;

            // Attempt to create the Windows font object
            try
            {
                windowsFont = new System.Drawing.Font( familyName, this.height,
                    System.Drawing.FontStyle.Regular );
            }
            catch
            {
                // Attempt to create the font using the "fallback" font family
                // defined in the Settings file
                Log.Write( "The desired font family was not available." );
            }

            d3dFont = new Direct3D.Font( renderer.Device, windowsFont );
            textSprite = new Direct3D.Sprite( renderer.Device );

            renderer.AddGraphicsObject( this );
        }
        #endregion

        #region Disposing
        public void Dispose()
        {
            if ( d3dFont != null )
                d3dFont.Dispose();

            d3dFont = null;

            if ( textSprite != null )
                textSprite.Dispose();
        }
        #endregion

        #region IGraphicsObject members
        public void OnDeviceReset()
        {
            if ( d3dFont != null )
                d3dFont.OnResetDevice();
        }

        public void OnDeviceLost()
        {
            if ( d3dFont != null )
                d3dFont.OnLostDevice();
        }
        #endregion

        #region Text rendering methods
        /// <summary>
        /// Renders text to the screen.
        /// </summary>
        /// <param name="position">The transformed screen-coordinates at which to render
        /// the text.</param>
        /// <param name="text">The string to render.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="shadowed">Should the text be rendered with a shadow?</param>
        public void RenderText( Vector2 position, string text, Color color, bool shadowed )
        {
            if ( textSprite == null || d3dFont == null || String.IsNullOrEmpty( text ) )
                return;

            try
            {
                try
                {
                    textSprite.Begin( SpriteFlags.AlphaBlend | SpriteFlags.SortTexture );

                    if ( shadowed )
                        d3dFont.DrawText( textSprite, text, ( int )position.X + shadowOffset,
                            ( int )position.Y + shadowOffset, shadowColor );

                    d3dFont.DrawText( textSprite, text, ( int )position.X, ( int )position.Y, color );
                }
                finally
                {
                    textSprite.End();
                }
            }
            catch ( Exception ex )
            {
                Log.Write( "Unable to render font: " + ex.ToString() );
            }
        }

        /// <summary>
        /// Renders text to the screen, in the color white.
        /// </summary>
        /// <param name="position">The transformed screen-coordinates at which to render
        /// the text.</param>
        /// <param name="text">The string to render.</param>
        /// <param name="shadowed">Should the text be rendered with a shadow?</param>
        public void RenderText( Vector2 position, string text, bool shadowed )
        {
            RenderText( position, text, Color.White, shadowed );
        }

        /// <summary>
        /// Renders centered text.
        /// </summary>
        /// <param name="position">The positon that will be used as the center of the text.</param>
        /// <param name="text">The string to render.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="shadowed">Should the text be rendered with a shadow?</param>
        public void RenderTextCentered( Vector2 position, string text, Color color, bool shadowed )
        {
            RenderText( new Vector2( position.X - text.Length * height * 3 / 10,
                position.Y - height / 2 ), text, color, shadowed );
        }
        #endregion
    }
}
