using System;
using Microsoft.DirectX;
using Gas.Helpers;
using Gas.Input;

namespace GamePrototype
{
    public class PlayerRobotControl : RobotControl
    {
        #region Variables
        private KeyboardDevice keyboard = null;
        private Config config = new Config( "Config.txt" );
        private readonly float moveForceMag = 0.0f;
        private Cursor crosshair = null;
        #endregion

        #region Constructor
        public PlayerRobotControl( Cursor crosshair )
        {
            keyboard = new KeyboardDevice( null );
            moveForceMag = config.GetSetting<float>( "RobotMoveForceMag" );
            this.crosshair = crosshair;
        }
        #endregion

        #region RobotControl methods
        public override void Update( float moveFactor )
        {
            if ( robot == null )
                return;

            keyboard.Update();

            Vector2 force = new Vector2();

            if ( keyboard.A || keyboard.Left )
                force += new Vector2( -1, 0 );
            if ( keyboard.D || keyboard.Right )
                force += new Vector2( 1, 0 );

            if ( keyboard.W || keyboard.Up )
                force += new Vector2( 0, 1 );
            if ( keyboard.S || keyboard.Down )
                force += new Vector2( 0, -1 );

            force.Normalize();
            force *= moveForceMag;

            robot.ApplyForce( force );

            Vector2 turretToCrosshair = crosshair.Center - robot.Position;
            float turretOrientation = ( float )Math.Atan2( ( float )turretToCrosshair.Y, ( float )turretToCrosshair.X )
                - ( float )Math.PI / 2.0f;

            robot.TurretOrientation = turretOrientation;

            if ( crosshair.LeftButtonPressed )
                robot.FireProjectile();

            if ( crosshair.RightButtonPressed )
                robot.FireExplodingProjectile();
        }

        public override void OnCollision( Entity hit )
        {

        }
        #endregion
    }
}