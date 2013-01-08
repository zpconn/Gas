using System;
using System.Collections.Generic;
using System.Text;
using Gas.Graphics;
using Gas.Helpers;
using Microsoft.DirectX;

namespace Water
{
    [MainVisualEffectClass]
    public class Water : VisualEffect
    {
        #region Variables
        private Effect effect = null;
        private HighResTimer timer = new HighResTimer();
        private float time = 0.0f;
        #endregion

        #region Constructor
        public Water( Renderer renderer )
            : base( renderer )
        {
            effect = new Effect( renderer, @".\\FXFiles\\Water.fx" );
        }
        #endregion

        #region VisualEffect interface
        public override void BeginRenderScene()
        {
            // Update the time by a little bit each frame
            timer.Update();
            time += 3.3f * timer.MoveFactorPerSecond;

            renderer.Begin( effect );
            renderer.SetPass( 0 );
        }

        public override void EndRenderScene()
        {
            renderer.End();
        }

        public override void BeginRenderObject( Material material )
        {
            effect.SetValue( "world", renderer.WorldMatrix );
            effect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );
            effect.SetValue( "water", material.Textures[ 0 ] );
            effect.SetValue( "sand", material.Textures[ 1 ] );
            effect.SetValue( "timer", time );

            if ( renderer.Lights.Count > 0 )
            {
                effect.SetValue( "lightPos", new Vector4( renderer.Lights[ 0 ].Position.X,
                    renderer.Lights[ 0 ].Position.Y, 1.0f, 1.0f ) );
                effect.SetValue( "lightColor", new Vector4( ( float )renderer.Lights[ 0 ].Color.R / 255.0f,
                    ( float )renderer.Lights[ 0 ].Color.G / 255.0f,
                    ( float )renderer.Lights[ 0 ].Color.B / 255.0f,
                    ( float )renderer.Lights[ 0 ].Color.A / 255.0f ) );
                effect.SetValue( "range", renderer.Lights[ 0 ].Range );
            }

            effect.CommitChanges();
        }

        public override void BeginRenderObject( Material material, List<object> extraData )
        {
            effect.SetValue( "world", renderer.WorldMatrix );
            effect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );
            effect.SetValue( "water", material.Textures[ 0 ] );
            effect.SetValue( "sand", material.Textures[ 1 ] );
            effect.SetValue( "timer", time );

            if ( extraData.Count > 0 )
            {
                Vector2 texCoordScroll = ( Vector2 )extraData[ 0 ];
                effect.SetValue( "texCoordScroll", new Vector4( texCoordScroll.X, texCoordScroll.Y, 0.0f, 0.0f ) );
            }

            if ( renderer.Lights.Count > 0 )
            {
                effect.SetValue( "lightPos", new Vector4( renderer.Lights[ 0 ].Position.X,
                    renderer.Lights[ 0 ].Position.Y, 1.0f, 1.0f ) );
                effect.SetValue( "lightColor", new Vector4( ( float )renderer.Lights[ 0 ].Color.R / 255.0f,
                    ( float )renderer.Lights[ 0 ].Color.G / 255.0f,
                    ( float )renderer.Lights[ 0 ].Color.B / 255.0f,
                    ( float )renderer.Lights[ 0 ].Color.A / 255.0f ) );
                effect.SetValue( "range", renderer.Lights[ 0 ].Range );
            }

            effect.CommitChanges();
        }

        public override void EndRenderObject()
        {
        }
        #endregion
    }
}