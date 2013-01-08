using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;
using Gas.Graphics;
using Gas.Input;
using Gas.Helpers;

namespace GamePrototype
{
    public class Game
    {
        #region Variables
        private Config config = new Config( "Config.txt" );

        private readonly float physicsTimeStep = 0.0f;
        private float accumulator = 0.0f;

        private FiniteStateMachine fsm = new FiniteStateMachine();
        private Renderer renderer = null;

        private Audio backgroundMusic = null;
        private Timer musicTimer = new Timer();
        #endregion

        #region Constructor
        public Game( Renderer renderer )
        {
            this.renderer = renderer;

            physicsTimeStep = config.GetSetting<float>( "PhysicsTimeStep" );

            SetupMusic();

            fsm.AddState( new SplashScreenState( renderer ) );
            fsm.AddState( new MainMenuState( renderer ) );
            fsm.AddState( new DuelState( renderer ) );
            fsm.DefaultStateID = ( int )StateTypes.SplashScreen;
        }
        #endregion

        #region Background Music
        private void SetupMusic()
        {
            backgroundMusic = Audio.FromFile( @".\\Music\\backgroundMusic.mp3" );
            musicTimer.Interval = ( int )backgroundMusic.Duration * 1000;
            musicTimer.Tick += new EventHandler( MusicTimerTick );
            musicTimer.Enabled = true;

            backgroundMusic.Play();
        }

        void MusicTimerTick( object sender, EventArgs e )
        {
            backgroundMusic.Stop();
            backgroundMusic.Play();
        }
        #endregion

        #region Interface
        public void Update( float moveFactor )
        {
            if ( !renderer.Windowed )
            {
                System.Windows.Forms.Cursor.Position = new Point( renderer.FullscreenSize.Width,
                    renderer.FullscreenSize.Height );
            }

            fsm.Render( moveFactor );

            if ( physicsTimeStep > 0.0f )
            {
                accumulator += moveFactor;

                while ( accumulator >= physicsTimeStep )
                {
                    fsm.Update( physicsTimeStep );
                    accumulator = -physicsTimeStep;
                }
            }
            else
                fsm.Update( moveFactor );
        }
        #endregion

        #region State Types
        private enum StateTypes : int
        {
            SplashScreen = 0,
            MainMenu = 1,
            DuelState = 2
        }
        #endregion

        #region SplashScreenState
        private class SplashScreenState : FSMState
        {
            #region Variables
            private Renderer renderer = null;
            private Surface splashScreen = null;
            private Light light = null;

            private const float LightAngularVelocity = ( 2.0f * ( float )Math.PI ) / 4.0f;
            private readonly float RotationTime = ( 2.0f * ( float )Math.PI ) / LightAngularVelocity;
            private float lightAngle = 0.0f;
            private float time = 0.0f;
            #endregion

            #region Constructor
            public SplashScreenState( Renderer renderer )
            {
                this.renderer = renderer;
                type = ( int )StateTypes.SplashScreen;
                splashScreen = new Surface( renderer, "splashScreen", renderer.FullscreenSize );
            }
            #endregion

            #region FSMState Methods
            public override void Enter()
            {
                time = 0.0f;
                light = renderer.RegisterNewLight( 150.0f, 1.0f );
            }

            public override void Exit()
            {
                renderer.RemoveLight( light );
            }

            public override void Update( float moveFactor )
            {
            }

            public override void Render( float moveFactor )
            {
                time += moveFactor;
                lightAngle += LightAngularVelocity * moveFactor;

                light.Position = Vector2.TransformCoordinate( new Vector2(), Matrix.Translation( 175.0f, 0, 0 ) *
                    Matrix.RotationZ( lightAngle ) );

                splashScreen.Render();
            }

            public override int CheckTransitions( float moveFactor )
            {
                if ( time >= RotationTime )
                    return ( int )StateTypes.MainMenu;

                return ( int )StateTypes.SplashScreen;
            }
            #endregion
        }
        #endregion

        #region MainMenuState
        private class MainMenuState : FSMState
        {
            #region Variables
            private Renderer renderer = null;
            private Surface background = null;

            private Light light = null;

            private Cursor cursor = null;
            private Menu mainMenu = null;

            private Config config = new Config( "Config.txt" );

            private int goalState = ( int )StateTypes.MainMenu;
            #endregion

            #region Constructor
            public MainMenuState( Renderer renderer )
            {
                type = ( int )StateTypes.MainMenu;
                this.renderer = renderer;

                background = new Surface( renderer, "mainMenuBg", renderer.FullscreenSize );

                cursor = new Cursor( renderer, "cursor", new Size( 50, 50 ) );

                mainMenu = new Menu( renderer, new Vector2( 0, -30 ), 80.0f, config.GetSetting<string>( "MainMenuFont" ),
                    cursor, "buttonPressed", "buttonUnpressed", new Size( 200, 50 ) );

                mainMenu.AddButton( "Duel", "Duel" );
                mainMenu.AddButton( "Survival", "Survival" );
                mainMenu.AddButton( "Quit", "Quit" );

                mainMenu.GetButton( "Duel" ).Pressed += new EventHandler( OnDuelPressed );
                mainMenu.GetButton( "Survival" ).Pressed += new EventHandler( OnSurvivalPressed );
                mainMenu.GetButton( "Quit" ).Pressed += new EventHandler( OnQuitButtonPressed );
            }
            #endregion

            #region FSMState Methods
            public override void Enter()
            {
                light = renderer.RegisterNewLight( 220.0f, 1.0f );
                goalState = ( int )StateTypes.MainMenu;
            }

            public override void Exit()
            {
                renderer.RemoveLight( light );
            }

            public override void Update( float moveFactor )
            {
            }

            public override void Render( float moveFactor )
            {
                cursor.Update( moveFactor );

                light.Position = new Vector2( cursor.Position.X, cursor.Position.Y );

                background.Render();
                mainMenu.Render();
                cursor.Render();
            }

            public override int CheckTransitions( float moveFactor )
            {
                return goalState;
            }
            #endregion

            #region Menu events
            void OnDuelPressed( object sender, EventArgs e )
            {
                goalState = ( int )StateTypes.DuelState;
            }

            void OnSurvivalPressed( object sender, EventArgs e )
            {
            }

            void OnQuitButtonPressed( object sender, EventArgs e )
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            #endregion
        }
        #endregion

        #region DuelState
        private class DuelState : FSMState
        {
            #region Variables
            private Renderer renderer = null;
            private Scene scene = null;
            #endregion

            #region Constructor
            public DuelState( Renderer renderer )
            {
                this.renderer = renderer;
                type = ( int )StateTypes.DuelState;

                ShadowManager.Initialize( renderer );

                scene = new Scene( renderer );
            }
            #endregion

            #region FSMState methods
            public override void Enter()
            {
                scene.Begin();
            }

            public override void Exit()
            {
                scene.End();
            }

            public override void Update( float moveFactor )
            {
                scene.Update( moveFactor );
            }

            public override void Render( float moveFactor )
            {
                scene.Render( moveFactor );
            }

            public override int CheckTransitions( float moveFactor )
            {
                return ( int )StateTypes.DuelState;
            }
            #endregion
        }
        #endregion
    }
}