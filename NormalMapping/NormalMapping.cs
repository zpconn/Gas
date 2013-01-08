using System;
using System.Collections.Generic;
using System.Text;
using Gas.Graphics;
using Microsoft.DirectX;

namespace NormalMapping
{
    [MainVisualEffectClass]
    public class NormalMapping : VisualEffect
    {
        #region Variables
        private Effect effect = null;
        #endregion

        #region Constructor
        public NormalMapping( Renderer renderer )
            : base( renderer )
        {
            effect = new Effect( renderer, @".\\FXFiles\\Lighting.fx" );
        }
        #endregion

        #region VisualEffect interface
        public override void BeginRenderScene()
        {
            renderer.Begin( effect );
            renderer.SetPass( 1 );
        }

        public override void EndRenderScene()
        {
            renderer.End();
        }

        public override void BeginRenderObject( Material material )
        {
            effect.SetValue( "world", renderer.WorldMatrix );
            effect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );
            effect.SetValue( "diffuseMap", material.Textures[ 0 ] );
            effect.SetValue( "normalMap", material.Textures[ 1 ] );

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
