using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Gas.Graphics;

namespace TextureMapping
{
    [MainVisualEffectClass]
    public class TextureMapping : VisualEffect
    {
        #region Variables
        private Gas.Graphics.Effect effect = null;
        #endregion

        #region Constructor
        public TextureMapping( Renderer renderer )
            : base( renderer )
        {
            effect = new Gas.Graphics.Effect( renderer, @".\\FXFiles\\Lighting.fx" );
        }
        #endregion

        #region VisualEffect interface
        public override void BeginRenderScene()
        {
            renderer.Begin( effect );
            renderer.SetPass( 5 );
        }

        public override void EndRenderScene()
        {
            renderer.End();
        }

        public override void BeginRenderObject( Gas.Graphics.Material material )
        {
            effect.SetValue( "world", renderer.WorldMatrix );
            effect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );
            effect.SetValue( "diffuseMap", material.Textures[ 0 ] );

            Vector4[] lightPos = new Vector4[ 4 ];
            Vector4[] lightColor = new Vector4[ 4 ];
            float[] range = new float[ 4 ];

            for ( int i = 0; i < renderer.Lights.Count && i < 4; ++i )
            {
                lightPos[ i ] = new Vector4( renderer.Lights[ i ].Position.X,
                    renderer.Lights[ i ].Position.Y, 1.0f, 1.0f );
                lightColor[ i ] = new Vector4( ( float )renderer.Lights[ i ].Color.R / 255.0f,
                    ( float )renderer.Lights[ i ].Color.G / 255.0f,
                    ( float )renderer.Lights[ i ].Color.B / 255.0f,
                    ( float )renderer.Lights[ i ].Color.A / 255.0f );
                range[ i ] = renderer.Lights[ i ].Range;
            }

            effect.SetValue( "lightPos", lightPos );
            effect.SetValue( "lightColor", lightColor );
            effect.SetValue( "range", range );
            effect.SetValue( "numActiveLights", renderer.Lights.Count );

            effect.CommitChanges();
        }

        public override void EndRenderObject()
        {
        }
        #endregion
    }
}
