using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;
using Gas.Graphics;
using Gas.Helpers;
using Gas.Input;
using Graphics = Gas.Graphics;

namespace Gas.Graphics
{
    /// <summary>
    /// This class applies bloom post-processing to the scene. All effects are rendered using
    /// pixel shaders. It requires vertex shader 1.1 and pixel shader 2.0.
    /// </summary>
    public class BloomPostProcessor
    {
        #region Variables
        private Renderer renderer = null;
        private Effect bloomEffect = null;

        private Texture sceneImage = null;
        private Texture brightPassTex = null;
        private Texture blurHorizontalTex = null;
        private Texture blurVerticalTex = null;
        private Texture finalBloomImage = null;

        private Mesh fullscreenQuad = null;

        private float blur = 0.0f;
        private float bloomScale = 1.0f;
        private float brightPassThreshold = 0.4f;
        #endregion

        #region Properties
        /// <summary>
        /// Sets the scene image, to which the bloom effect is applied during rendering.
        /// </summary>
        public Texture SceneImage
        {
            set
            {
                sceneImage = value;
            }
        }

        /// <summary>
        /// Gets and sets the blur amount. The amount of visual blur is inversely proportional to this
        /// value.
        /// </summary>
        public float Blur
        {
            get
            {
                return blur;
            }
            set
            {
                blur = value;
            }
        }

        /// <summary>
        /// Gets and sets the bloom scale.
        /// </summary>
        public float BloomScale
        {
            get
            {
                return bloomScale;
            }
            set
            {
                bloomScale = value;
            }
        }

        /// <summary>
        /// Gets and sets the bright pass threshold. This is essentially the minimum brightness
        /// that a pixel must possess to be bloomed.
        /// 
        /// The brightness of a pixel is defined as follows:
        /// 
        /// brightness = (1/3) * R + (1/3) * G + (1/3) * B = (1/3) * (R + G + B)
        /// 
        /// This value always falls in the range [0,1].
        /// </summary>
        public float BrightPassThreshold
        {
            get
            {
                return brightPassThreshold;
            }
            set
            {
                brightPassThreshold = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the BloomPostProcessor.
        /// </summary>
        public BloomPostProcessor( Renderer renderer )
        {
            if ( renderer == null )
                throw new ArgumentNullException( "renderer", "Can't create the BloomPostProcessor with a "
                    + "null Renderer reference." );

            this.renderer = renderer;

            brightPassTex = new Texture( renderer, renderer.FullscreenSize.Width / 2,
                renderer.FullscreenSize.Height / 2, true );
            blurHorizontalTex = new Texture( renderer, renderer.FullscreenSize.Width / 2,
                renderer.FullscreenSize.Height / 2, true );
            blurVerticalTex = new Texture( renderer, renderer.FullscreenSize.Width / 2,
                renderer.FullscreenSize.Height / 2, true );
            finalBloomImage = new Texture( renderer, renderer.FullscreenSize.Width,
                renderer.FullscreenSize.Height, true );

            fullscreenQuad = Mesh.Rectangle( renderer, Color.Black, renderer.FullscreenSize.Width,
                renderer.FullscreenSize.Height );

            bloomEffect = GlobalResourceCache.CreateEffectFromFile( renderer,
                @".\\FXFiles\\Bloom.fx" );
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Applies the bloom effect to the scene image, and then renders the resulting bloomed
        /// image to the screen.
        /// </summary>
        public void Render()
        {
            // If sceneImage is null, then this method will fail utterly. Something's gone wrong if
            // it's null...
            System.Diagnostics.Debug.Assert( sceneImage != null );

            renderer.WorldMatrix = Matrix.Identity;

            // Remember the render target so we can revert back to it when we're done.
            renderer.SaveRenderTarget();

            renderer.Begin( bloomEffect );
            SetEffectParameters();

            // Render all the passes
            DoBrightPass();
            DoBlurHorizontalPass();
            DoBlurVerticalPass();
            DoFinalizePass();

            renderer.End();
        }

        /// <summary>
        /// Sets the bloom effect parameters to the values stored in this class.
        /// </summary>
        private void SetEffectParameters()
        {
            bloomEffect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );
            bloomEffect.SetValue( "blur", blur );
            bloomEffect.SetValue( "bloomScale", bloomScale );
            bloomEffect.SetValue( "brightPassThreshold", brightPassThreshold );
        }

        /// <summary>
        /// Finalizes the rendering technique by combining the bloom image with the scene image, and 
        /// rendering the composite to the screen.
        /// </summary>
        private void DoFinalizePass()
        {
            renderer.RestoreRenderTarget();
            bloomEffect.SetValue( "additiveBlendTex1", sceneImage );
            bloomEffect.SetValue( "additiveBlendTex2", blurVerticalTex );
            renderer.SetPass( 3 );
            fullscreenQuad.Render();
        }

        /// <summary>
        /// Renders the vertical Gaussian blur pass.
        /// </summary>
        private void DoBlurVerticalPass()
        {
            blurVerticalTex.SetAsRenderTarget();
            renderer.Clear();
            bloomEffect.SetValue( "bloomTex", blurHorizontalTex );
            renderer.SetPass( 2 );
            fullscreenQuad.Render();
        }

        /// <summary>
        /// Renders the horizontal Gaussian blur pass.
        /// </summary>
        private void DoBlurHorizontalPass()
        {
            blurHorizontalTex.SetAsRenderTarget();
            renderer.Clear();
            bloomEffect.SetValue( "bloomTex", brightPassTex );
            renderer.SetPass( 1 );
            fullscreenQuad.Render();
        }

        /// <summary>
        /// Renders the bright pass.
        /// </summary>
        private void DoBrightPass()
        {
            brightPassTex.SetAsRenderTarget();
            renderer.Clear();
            bloomEffect.SetValue( "brightPassTex", sceneImage );
            renderer.SetPass( 0 );
            fullscreenQuad.Render();
        }
        #endregion
    }
}