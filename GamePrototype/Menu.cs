using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Input;

namespace GamePrototype
{
    public class Cursor
    {
        #region Variables
        private MouseDevice mouse = null;
        private Surface surface = null;
        private Size size = new Size();

        private Renderer renderer = null;

        private Vector3 pos = new Vector3();
        private float movementScaleFactor = 1.0f;

        private bool clicked = false;

        private bool clickedLastUpdate = false;
        #endregion

        #region Properties
        public bool LeftButtonPressed
        {
            get
            {
                return mouse.LeftButtonPressed;
            }
        }

        public bool RightButtonPressed
        {
            get
            {
                return mouse.RightButtonPressed;
            }
        }

        public bool LeftButtonJustPressed
        {
            get
            {
                return ( !clickedLastUpdate && mouse.LeftButtonPressed );
            }
        }

        public Vector3 Position
        {
            get
            {
                return pos + new Vector3( surface.Size.Width * 2, surface.Size.Height * 2, 0.0f );
            }
            set
            {
                pos = value;
            }
        }

        public Vector2 Center
        {
            get
            {
                return new Vector2( pos.X + surface.Size.Width, pos.Y + surface.Size.Height );
            }
            set
            {
                pos = new Vector3( value.X - surface.Size.Width, value.Y - surface.Size.Height, 0.0f );
            }
        }

        public Size Size
        {
            get
            {
                return size;
            }
        }
        #endregion

        #region Events
        public event EventHandler Clicked;
        public event EventHandler Unclicked;
        #endregion

        #region Constructor
        public Cursor( Renderer renderer, string materialName, Size size )
        {
            this.renderer = renderer;

            this.size = size;
            surface = new Surface( renderer, materialName, size );

            mouse = new MouseDevice( null );
        }

        public Cursor( Renderer renderer, string materialName, Size size,
            float movementScaleFactor )
        {
            this.renderer = renderer;

            this.size = size;
            this.movementScaleFactor = movementScaleFactor;
            surface = new Surface( renderer, materialName, size );

            mouse = new MouseDevice( null );
        }
        #endregion

        #region Public methods
        public void Update( float moveFactor )
        {
            clickedLastUpdate = mouse.LeftButtonPressed;
            mouse.Update();

            pos.X -= mouse.MovementVector.X * movementScaleFactor;
            pos.Y += mouse.MovementVector.Y * movementScaleFactor;
            pos.Z += mouse.MovementVector.Z * movementScaleFactor;

            Matrix inverseProjView = Matrix.Invert( renderer.ProjectionMatrix ) * Matrix.Invert( renderer.ViewMatrix );
            Vector3 screenPos = Vector3.TransformCoordinate( pos, renderer.ViewMatrix * renderer.ProjectionMatrix );
            Vector2 worldSpaceUpRightBounds = Vector2.TransformCoordinate( new Vector2( 1, 1 ), inverseProjView );
            Vector2 worldSpaceBottomLeftBounds = Vector2.TransformCoordinate( new Vector2( -1, -1 ), inverseProjView );

            if ( screenPos.X < -1 )
                pos.X = worldSpaceBottomLeftBounds.X;
            else if ( screenPos.X > 1 )
                pos.X = worldSpaceUpRightBounds.X;

            if ( screenPos.Y < -1 )
                pos.Y = worldSpaceBottomLeftBounds.Y;
            else if ( screenPos.Y > 1 )
                pos.Y = worldSpaceUpRightBounds.Y;

            if ( mouse.LeftButtonPressed )
            {
                if ( !clicked && Clicked != null )
                    Clicked( this, null );

                clicked = true;
            }
            else
            {
                if ( clicked && Clicked != null )
                    Unclicked( this, null );

                clicked = false;
            }
        }

        public void Render()
        {
            surface.Render( new Vector3( pos.X + surface.Size.Width,
                pos.Y + surface.Size.Height, -1.0f ) );
        }
        #endregion
    }

    public class Menu
    {
        #region Button class
        public class Button
        {
            #region Variables
            private Vector2 position = new Vector2();
            private string text = "";
            private Cursor cursor = null;

            private bool pressed = false;

            private Surface unpressedSurf = null;
            private Surface pressedSurf = null;
            private Surface textSurf = null;

            private Gas.Graphics.Font font = null;

            private Renderer renderer = null;
            #endregion

            #region Events
            public event EventHandler Pressed;
            #endregion

            #region Constructor
            public Button( Renderer renderer, Cursor cursor, Gas.Graphics.Font font, Surface unpressedSurf,
                Surface pressedSurf, string text, Vector2 position )
            {
                this.renderer = renderer;
                this.cursor = cursor;
                this.font = font;
                this.unpressedSurf = unpressedSurf;
                this.pressedSurf = pressedSurf;
                this.text = text;
                this.position = position;

                this.cursor.Clicked += new EventHandler( OnCursorClicked );
                this.cursor.Unclicked += new EventHandler( OnCursorUnclicked );

                CreateTextSurface( text );
            }
            #endregion

            #region Cursor events
            void OnCursorUnclicked( object sender, EventArgs e )
            {
                Point cursorPos = new Point( ( int )cursor.Position.X, ( int )cursor.Position.Y );
                Rectangle buttonRect = new Rectangle( ( int )position.X, ( int )position.Y,
                    pressedSurf.Size.Width, pressedSurf.Size.Height );

                if ( buttonRect.Contains( cursorPos ) && pressed )
                    if ( Pressed != null )
                        Pressed( this, null );

                pressed = false;
            }

            void OnCursorClicked( object sender, EventArgs e )
            {
                Point cursorPos = new Point( ( int )cursor.Position.X, ( int )cursor.Position.Y );
                Rectangle buttonRect = new Rectangle( ( int )position.X, ( int )position.Y,
                    pressedSurf.Size.Width, pressedSurf.Size.Height );

                if ( buttonRect.Contains( cursorPos ) )
                    pressed = true;
            }
            #endregion

            #region Public methods
            public void Render()
            {
                if ( pressed )
                {
                    pressedSurf.Render( new Vector3( position.X, position.Y, -0.5f ) );
                    textSurf.Render( new Vector3( position.X - 2.0f, position.Y + 4.5f, -0.75f ) );
                }
                else
                {
                    unpressedSurf.Render( new Vector3( position.X, position.Y, -0.5f ) );
                    textSurf.Render( new Vector3( position.X - 2.0f, position.Y + 4.5f, -0.75f ) );
                }
            }
            #endregion

            #region Create Text Surface
            private void CreateTextSurface( string text )
            {
                Material textMat = new Material( renderer );
                textMat.VisualEffectName = "lighting";

                Texture textTex = new Texture( renderer, pressedSurf.Size.Width, pressedSurf.Size.Height,
                    true );

                renderer.SaveRenderTarget();
                textTex.SetAsRenderTarget();
                renderer.Clear( Microsoft.DirectX.Direct3D.ClearFlags.Target, Color.FromArgb( 0, 0, 0, 0 ),
                    1.0f, 0 );

                renderer.Begin( null );
                font.RenderTextCentered( new Vector2( pressedSurf.Size.Width / 2, pressedSurf.Size.Height / 2 ),
                    text, Color.White, false );
                renderer.End();

                renderer.RestoreRenderTarget();

                textMat.Textures[ 0 ] = textTex;
                renderer.AddMaterial( textMat, typeof( Button ).ToString() + ". Text: " + text );

                textSurf = new Surface( renderer, typeof( Button ).ToString() + ". Text: " + text,
                    pressedSurf.Size );
            }
            #endregion
        }
        #endregion

        #region Variables
        private Renderer renderer = null;
        private Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        private Vector2 position = new Vector2();
        private float buttonYSpacing = 30.0f;
        private Gas.Graphics.Font textFont = null;
        private Cursor cursor = null;
        private Surface unpressedSurf = null;
        private Surface pressedSurf = null;
        #endregion

        #region Constructor
        public Menu( Renderer renderer, Vector2 position, float buttonYSpacing, string fontName,
            Cursor cursor, string pressedMaterialName, string unpressedMaterialName, Size buttonSize )
        {
            this.renderer = renderer;
            this.position = position;
            this.buttonYSpacing = buttonYSpacing;
            this.cursor = cursor;

            textFont = new Gas.Graphics.Font( renderer, fontName, 16 );

            unpressedSurf = new Surface( renderer, unpressedMaterialName, buttonSize );
            pressedSurf = new Surface( renderer, pressedMaterialName, buttonSize );
        }
        #endregion

        #region Public methods
        public void AddButton( string name, string text )
        {
            Vector2 buttonPos = position + buttons.Count * ( new Vector2( 0.0f, -buttonYSpacing ) );
            buttons.Add( name, new Button( renderer, cursor, textFont, unpressedSurf, pressedSurf, text,
                buttonPos ) );
        }

        public Button GetButton( string name )
        {
            return buttons[ name ];
        }

        public void Render()
        {
            foreach ( Button button in buttons.Values )
            {
                button.Render();
            }
        }
        #endregion
    }
}