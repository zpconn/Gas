using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    public partial class GamePrototypeForm : GraphicsForm
    {
        #region Variables
        private Config config = new Config( "Config.txt" );

        private Gas.Graphics.Font fpsFont;

        private bool showFPS = false;

        private Game game = null;

        private Texture sceneTex = null;
        private BloomPostProcessor bloomProcessor = null;
        private bool useBloom = true;
        #endregion

        #region GraphicsForm Methods
        protected override void InitializeGame()
        {
            System.Windows.Forms.Cursor.Hide();

            renderer.ProjectionMode = ProjectionMode.Orthogonal;
            renderer.ViewMatrix = Matrix.LookAtLH( new Vector3( 0, 0, -5.0f ), new Vector3(),
                new Vector3( 0, 1, 0 ) );

            showFPS = config.GetSetting<bool>( "ShowFPS" );

            fpsFont = new Gas.Graphics.Font( renderer, "Arial", 16 );
            fpsFont.ShadowColor = Color.Gray;

            game = new Game( renderer );

            useBloom = config.GetSetting<bool>( "UseBloom" );

            sceneTex = new Texture( renderer, renderer.FullscreenSize.Width, renderer.FullscreenSize.Height,
                true );
            bloomProcessor = new BloomPostProcessor( renderer );
            bloomProcessor.Blur = config.GetSetting<float>( "BloomBlur" );
            bloomProcessor.BloomScale = config.GetSetting<float>( "BloomScale" );
            bloomProcessor.BrightPassThreshold = config.GetSetting<float>( "BloomBrightPassThreshold" );

            this.KeyDown += new KeyEventHandler( OnKeyDown );
        }

        void OnKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Escape )
                running = false;
        }

        protected override void UpdateEnvironment()
        {

        }

        protected override void Render3DEnvironment()
        {
            renderer.Clear();

            if ( useBloom )
            {
                renderer.SaveRenderTarget();
                sceneTex.SetAsRenderTarget();
                renderer.Clear();
            }

            renderer.Render();
            game.Update( timer.MoveFactorPerSecond );

            if ( useBloom )
            {
                renderer.SetScreenAsRenderTarget();

                Matrix oldView = renderer.ViewMatrix;
                renderer.ViewMatrix = Matrix.LookAtLH( new Vector3( 0, 0, -5.0f ), new Vector3(),
                    new Vector3( 0, 1, 0 ) );

                bloomProcessor.SceneImage = sceneTex;
                bloomProcessor.Render();

                renderer.ViewMatrix = oldView;
            }

            if ( showFPS )
            {
                renderer.Begin( null );

                fpsFont.RenderText( new Vector2( 20, 20 ), "FPS: " + timer.FramesPerSecond.ToString(),
                    Color.White, true );

                renderer.End();
            }

            renderer.Present();
        }
        #endregion
    }
}