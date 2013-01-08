using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    public class Scene
    {
        #region Variables
        private Renderer renderer = null;
        private EntityArbiter entityArbiter = new EntityArbiter();
        private Cursor crosshair = null;

        private Size arenaSize = new Size();

        private Robot playerBot = null;
        private Robot cpuBot = null;

        private Light playerBotLight = null;
        private Light cpuBotLight = null;

        private Surface arenaGround = null;
        private Surface lava = null;

        private float lavaTexCoordTiling = 1.0f;

        private Vector2 playerHUDPos = new Vector2();
        private Vector2 cpuHUDPos = new Vector2();

        private Gas.Graphics.Font statsFont = null;

        private Camera camera = null;
        private string cameraMode = "Floating";

        private Config config = new Config( "Config.txt" );

        private Vector2 focusPointLastFrame = new Vector2();

        private string aimMode = null;
        private bool shadows = false;
        #endregion

        #region Properties
        public Vector2 PlayerRobotPosition
        {
            get
            {
                return playerBot.Position;
            }
        }

        public Vector2 PlayerRobotVelocity
        {
            get
            {
                return playerBot.Velocity;
            }
        }

        public Vector2 PlayerRobotAcceleration
        {
            get
            {
                return playerBot.Acceleration;
            }
        }
        #endregion

        #region Constructor
        public Scene( Renderer renderer )
        {
            this.renderer = renderer;

            camera = new Camera( renderer );

            crosshair = new Cursor( renderer, "crosshair", new Size( 50, 49 ) );

            arenaSize = new Size( config.GetSetting<int>( "ArenaWidth" ), config.GetSetting<int>( "ArenaHeight" ) );

            aimMode = config.GetSetting<string>( "AimMode" );
            cameraMode = config.GetSetting<string>( "CameraMode" );
            shadows = config.GetSetting<bool>( "Shadows" );

            if ( aimMode != "Relative" && aimMode != "Absolute" )
            {
                Log.Write( "Invalid value for setting 'AimMode'. Defaulting to AimMode = 'Absolute'." );
                aimMode = "Absolute";
            }

            if ( cameraMode != "Fixed" && cameraMode != "Floating" )
            {
                Log.Write( "Invalid value for setting 'CameraMode'. Defaulting to CameraMode = 'Floating'." );
                cameraMode = "Floating";
            }

            lavaTexCoordTiling = config.GetSetting<float>( "LavaTexCoordTiling" );

            arenaGround = new Surface( renderer, "arenaGround", arenaSize, 2.0f );
            lava = new Surface( renderer, "lava", new Size( renderer.FullscreenSize.Width, renderer.FullscreenSize.Height ),
                lavaTexCoordTiling );

            playerBot = new Robot( renderer, new PlayerRobotControl( crosshair ), this );
            cpuBot = new Robot( renderer, new CPURobotControl( this ), this );

            statsFont = new Gas.Graphics.Font( renderer, "Arial", 14 );
            statsFont.ShadowColor = Color.Black;

            playerBot.Position = new Vector2( 500.0f, 0.0f );
            cpuBot.Position = new Vector2( -500.0f, 0.0f );

            entityArbiter.AddEntity( playerBot );
            entityArbiter.AddEntity( cpuBot );

            PopulateArenaWithObstacles();
        }
        #endregion

        #region Public interface
        public void Begin()
        {
            playerBotLight = renderer.RegisterNewLight( 200.0f, 1.0f, new Vector2(), Color.FromArgb( 255, 255, 0 ) );
            cpuBotLight = renderer.RegisterNewLight( 200.0f, 1.0f, new Vector2(), Color.FromArgb( 255, 0, 255 ) );
        }

        public void End()
        {
            renderer.RemoveLight( playerBotLight );
            renderer.RemoveLight( cpuBotLight );
        }

        public void Update( float moveFactor )
        {
            entityArbiter.Update( moveFactor );
            if ( aimMode == "Relative" )
                crosshair.Center = crosshair.Center + ( camera.FocusPoint - focusPointLastFrame );
        }

        public void Render( float moveFactor )
        {
            playerBotLight.Position = playerBot.Position;
            cpuBotLight.Position = cpuBot.Position;

            focusPointLastFrame = camera.FocusPoint;
            camera.FocusPoint = CalculateFocusPoint();

            Vector2 scrollVec = new Vector2(
                camera.FocusPoint.X / ( lava.Size.Width ) * lavaTexCoordTiling,
                -camera.FocusPoint.Y / ( lava.Size.Height ) * lavaTexCoordTiling );
            List<object> extraData = new List<object>();
            extraData.Add( ( object )scrollVec );
            lava.Render( new Vector3( camera.FocusPoint.X, camera.FocusPoint.Y, 1.1f ), extraData );

            arenaGround.Render( new Vector3( 0, 0, 1.0f ) );

            RenderHUD();

            crosshair.Update( moveFactor );

            entityArbiter.Render( moveFactor );
            crosshair.Render();

            if ( shadows )
                ShadowManager.RenderAllShadows();
        }

        public void AddEntity( Entity entity )
        {
            entityArbiter.AddEntity( entity );
        }
        #endregion

        #region HUD
        private void RenderHUD()
        {
            #region Calculate CPU robot screen pos
            Vector3 cpuBotScreenPos = Vector3.TransformCoordinate( new Vector3( cpuBot.Position.X, cpuBot.Position.Y, 0.0f ),
                renderer.ViewMatrix * renderer.ProjectionMatrix );

            // The coordinates are now in the normalized range [-1,1]. We want them in the normalized range [0,1].
            cpuBotScreenPos.X = ( cpuBotScreenPos.X + 1.0f ) / 2.0f;
            cpuBotScreenPos.Y = ( cpuBotScreenPos.Y + 1.0f ) / 2.0f;
            #endregion

            #region Calculate Player robot screen pos
            Vector3 playerBotScreenPos = Vector3.TransformCoordinate( new Vector3( playerBot.Position.X, playerBot.Position.Y, 0.0f ),
                renderer.ViewMatrix * renderer.ProjectionMatrix );

            // The coordinates are now in the normalized range [-1,1]. We want them in the normalized range [0,1].
            playerBotScreenPos.X = ( playerBotScreenPos.X + 1.0f ) / 2.0f;
            playerBotScreenPos.Y = ( playerBotScreenPos.Y + 1.0f ) / 2.0f;
            #endregion

            statsFont.RenderTextCentered( new Vector2( renderer.FullscreenSize.Width * playerBotScreenPos.X,
                renderer.FullscreenSize.Height * ( 1.0f - playerBotScreenPos.Y ) - 80 ),
                playerBot.Health.ToString(), Color.Gray, true );

            statsFont.RenderTextCentered( new Vector2( renderer.FullscreenSize.Width * cpuBotScreenPos.X,
                renderer.FullscreenSize.Height * ( 1.0f - cpuBotScreenPos.Y ) - 80 ),
                cpuBot.Health.ToString(), Color.Gray, true );
        }

        private Vector2 CalculateFocusPoint()
        {
            Vector2 focusPoint = new Vector2();

            if ( cameraMode == "Floating" )
            {
                float maxDistance = Math.Min( renderer.FullscreenSize.Width, renderer.FullscreenSize.Height ) - 100;

                float distance = Math.Min( Vector2.Length( playerBot.Position - cpuBot.Position ), maxDistance ) / 2.0f;

                Vector2 displacement = Vector2.Normalize( cpuBot.Position - playerBot.Position );
                displacement *= distance;

                focusPoint = playerBot.Position + displacement;
            }
            else if ( cameraMode == "Fixed" )
            {
                focusPoint = playerBot.Position;
            }

            return focusPoint;
        }
        #endregion

        #region Arena generation
        private void PopulateArenaWithObstacles()
        {
            int numObstacles = config.GetSetting<int>( "NumObstacles" );
            float obstacleRadius = config.GetSetting<float>( "ObstacleRadius" );
            float obstacleYOffset = config.GetSetting<float>( "ObstacleYOffset" );

            Random rand = new Random();

            Obstacle.ObstacleMesh = Mesh.Circle( renderer, Color.White, obstacleRadius, 24 );

            float obstacleY = arenaSize.Height / 2 - obstacleRadius * 2;
            for ( int i = 0; i < numObstacles; ++i )
            {
                entityArbiter.AddEntity( new Obstacle(
                    renderer, false, "obstacle",
                    new Vector2(
                    rand.Next( -arenaSize.Width / 2 + ( int )obstacleRadius * 2, arenaSize.Width / 2 - ( int )obstacleRadius * 2 ),
                    obstacleY ) ) );

                obstacleY -= 2 * obstacleRadius + obstacleYOffset;
            }
        }
        #endregion
    }
}
