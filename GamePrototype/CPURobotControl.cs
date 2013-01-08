using System;
using Microsoft.DirectX;
using Gas.Helpers;

namespace GamePrototype
{
    public class CPURobotControl : RobotControl
    {
        #region Variables
        private Scene scene = null;

        private Config config = new Config( "Config.txt" );
        private readonly float moveForceMag = 0.0f;
        private readonly float explodingProjProbability = 0.0f;

        private readonly int targetingPrecision = 10;
        #endregion

        #region Constructor
        public CPURobotControl( Scene scene )
        {
            this.scene = scene;

            moveForceMag = config.GetSetting<float>( "RobotMoveForceMag" );

            targetingPrecision = config.GetSetting<int>( "TargetingPrecision" );
            explodingProjProbability = config.GetSetting<float>( "ExplodingProjectileProbability" );
        }
        #endregion

        #region RobotControl methods
        public override void Update( float moveFactor )
        {
            if ( robot == null )
                return;

            Vector2 turretToTarget = scene.PlayerRobotPosition - robot.Position;
            turretToTarget.Normalize();
            turretToTarget *= moveForceMag * 0.5f;

            robot.ApplyForce( turretToTarget );

            FireProjectile();

            Random rand = new Random();
            if ( rand.NextDouble() < explodingProjProbability )
                FireExplodingProjectile();
        }
        #endregion

        #region Targeting
        private void FireProjectile()
        {
            robot.TurretOrientation = ComputeFiringAngle() - ( float )Math.PI / 2.0f;
            robot.FireProjectile();
        }

        private void FireExplodingProjectile()
        {
            robot.TurretOrientation = ComputeFiringAngle() - ( float )Math.PI / 2.0f;
            robot.FireExplodingProjectile();
        }

        private float ComputeFiringAngle()
        {
            float impactTime = ComputeImpactTime( targetingPrecision );
            Vector2 targetPos = PredictTargetPosition( impactTime );
            return ComputeFiringAngle( targetPos );
        }

        private float ComputeFiringAngle( Vector2 targetPos )
        {
            Vector2 relPos = targetPos - robot.Position;
            return ( float )Math.Atan2( ( float )relPos.Y, ( float )relPos.X );
        }

        private float ComputeImpactTime( int numIterations )
        {
            float projectileStaticSpeed = config.GetSetting<float>( "ProjectileSpeed" );
            float projectileSpeed = projectileStaticSpeed;
            float timeEstimate = Vector2.Length( scene.PlayerRobotPosition - robot.Position ) / projectileSpeed;
            Vector2 targetPosEstimate = scene.PlayerRobotPosition;

            for ( int i = 0; i < numIterations; ++i )
            {
                targetPosEstimate = PredictTargetPosition( timeEstimate );

                float angle = ComputeFiringAngle( targetPosEstimate );
                Vector2 projectileVelocity = robot.Velocity + projectileStaticSpeed *
                    ( new Vector2( ( float )Math.Cos( angle ), ( float )Math.Sin( angle ) ) );
                projectileSpeed = projectileVelocity.Length();

                timeEstimate = Vector2.Length( targetPosEstimate - robot.Position ) / projectileSpeed;
            }

            return timeEstimate;
        }

        private Vector2 PredictTargetPosition( float timeDelta )
        {
            return scene.PlayerRobotPosition + scene.PlayerRobotVelocity * timeDelta;
        }
        #endregion
    }
}