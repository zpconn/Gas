using System;
using System.Drawing;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    public class RobotControl
    {
        #region Variables
        protected Robot robot = null;
        #endregion

        #region Public interface
        public virtual void Update( float moveFactor ) { }
        public virtual void OnCollision( Entity hit ) { }

        public void AttachToRobot( Robot robot )
        {
            this.robot = robot;
        }
        #endregion
    }

    public class Robot : Entity
    {
        #region Variables
        private Vector2 forces = new Vector2();
        private Surface turret = null;

        private Config config = new Config( "Config.txt" );

        private readonly float frictionalCoefficient = 0.0f;

        private readonly float projectileFireInterval = 0.0f;
        private float timeSinceLastProjFire = 0.0f;

        private readonly float explodingProjFireInterval = 0.0f;
        private float timeSinceLastExplodingProjFire = 0.0f;

        private RobotControl control = new RobotControl();

        private int health = 100;

        private readonly float coefficientOfRestitution = 0.0f;

        private float turretOrientation = 0.0f;

        private readonly Scene scene = null;
        #endregion

        #region Properties
        public int Health
        {
            get
            {
                return health;
            }
        }

        public float TurretOrientation
        {
            get
            {
                return turretOrientation;
            }
            set
            {
                turretOrientation = value;
            }
        }
        #endregion

        #region Constructor
        public Robot( Renderer renderer, Scene scene )
            : base( renderer, EntityType.Robot )
        {
            mesh = Mesh.Circle( renderer, Color.White, 60, 24 );
            turret = new Surface( renderer, "turret", new Size( 20, 75 ) );
            this.scene = scene;

            frictionalCoefficient = config.GetSetting<float>( "FrictionalCoefficient" );
            mass = config.GetSetting<float>( "RobotMass" );
            coefficientOfRestitution = config.GetSetting<float>( "CoefficientOfRestitution" );

            projectileFireInterval = config.GetSetting<float>( "ProjectileFireInterval" );
            timeSinceLastProjFire = projectileFireInterval;

            explodingProjFireInterval = config.GetSetting<float>( "ExplodingProjectileFireInterval" );
            timeSinceLastExplodingProjFire = explodingProjFireInterval;

            health = config.GetSetting<int>( "RobotStartHealth" );
        }

        public Robot( Renderer renderer, RobotControl control, Scene scene )
            : base( renderer, EntityType.Robot )
        {
            mesh = Mesh.Circle( renderer, Color.White, 60, 24 );
            turret = new Surface( renderer, "turret", new Size( 20, 75 ) );
            this.scene = scene;

            frictionalCoefficient = config.GetSetting<float>( "FrictionalCoefficient" );
            mass = config.GetSetting<float>( "RobotMass" );
            coefficientOfRestitution = config.GetSetting<float>( "CoefficientOfRestitution" );

            projectileFireInterval = config.GetSetting<float>( "ProjectileFireInterval" );
            timeSinceLastProjFire = projectileFireInterval;

            explodingProjFireInterval = config.GetSetting<float>( "ExplodingProjectileFireInterval" );
            timeSinceLastExplodingProjFire = explodingProjFireInterval;

            health = config.GetSetting<int>( "RobotStartHealth" );

            AttachController( control );
        }
        #endregion

        #region Public interface
        public void ApplyForce( Vector2 force )
        {
            forces += force;
        }

        public void AttachController( RobotControl control )
        {
            this.control = control;
            control.AttachToRobot( this );
        }

        public void ApplyDamage( int damageAmount )
        {
            health -= damageAmount;
        }

        public void FireProjectile()
        {
            if ( timeSinceLastProjFire < projectileFireInterval )
                return;

            timeSinceLastProjFire = 0.0f;

            Vector2 direction = new Vector2( ( float )Math.Cos( turretOrientation + ( float )Math.PI / 2.0f ),
                ( float )Math.Sin( turretOrientation + ( float )Math.PI / 2.0f ) );
            Vector2 turretTip = position + direction * turret.Size.Height;
            scene.AddEntity( new Projectile( renderer, turretTip, direction, velocity ) );
        }

        public void FireExplodingProjectile()
        {
            if ( timeSinceLastExplodingProjFire < explodingProjFireInterval )
                return;

            timeSinceLastExplodingProjFire = 0.0f;

            Vector2 direction = new Vector2( ( float )Math.Cos( turretOrientation + ( float )Math.PI / 2.0f ),
                ( float )Math.Sin( turretOrientation + ( float )Math.PI / 2.0f ) );
            Vector2 turretTip = position + direction * turret.Size.Height;
            scene.AddEntity( new ExplodingProjectile( renderer, turretTip, direction, velocity, scene ) );
        }
        #endregion

        #region Entity Methods
        public override void Update( float moveFactor )
        {
            if ( control != null )
                control.Update( moveFactor );

            // Update firing timers
            timeSinceLastProjFire += moveFactor;
            timeSinceLastExplodingProjFire += moveFactor;

            // Integrate velocity using the Euler scheme

            velocity += ( forces * ( 1.0f / mass ) ) * moveFactor;
            velocity += Vector2.Normalize( -velocity ) * ( frictionalCoefficient / mass ) * moveFactor;
            forces = new Vector2();

            base.Update( moveFactor );
        }

        public override void Render( float moveFactor )
        {
            renderer.AddRenderPacket( new RenderPacket( mesh, "robot", worldTransform ) );

            Matrix turretWorldTransform =
                Matrix.Translation( 0.0f, turret.Size.Height / 2, 0.0f ) *
                Matrix.RotationZ( turretOrientation ) *
                Matrix.Translation( position.X, position.Y, 0.0f );

            turret.Render( turretWorldTransform );
        }

        public override void OnCollision( Entity hit )
        {
            if ( control != null )
                control.OnCollision( hit );

            if ( !( hit.Type == EntityType.Robot || hit.Type == EntityType.Obstacle ) )
                return;

            #region Resolve interpenetrations
            Vector2 slideVec = this.Position - hit.Position;
            float stepSize = 1.0f;
            slideVec.Normalize();
            const int MaxIterations = 75;

            bool colliding = colliding = this.mesh.Intersects( hit.Mesh, this.WorldTransform, hit.WorldTransform );
            for ( int i = 0; i < MaxIterations && colliding; ++i )
            {
                this.position += slideVec * stepSize;
                worldTransform = Matrix.RotationZ( orientation ) * Matrix.Translation( position.X, position.Y, 0.0f );
                colliding = this.mesh.Intersects( hit.Mesh, this.WorldTransform, hit.WorldTransform );
            }
            #endregion

            #region Apply impulse
            Vector2 relVel = this.velocity - hit.Velocity;
            Vector2 normal = Vector2.Normalize( hit.Position - this.position );
            float impulseMag = Vector2.Dot( -( 1.0f + coefficientOfRestitution ) * relVel, normal )
                / ( 1.0f / this.mass + 1.0f / hit.Mass );

            this.velocity += normal * ( impulseMag / this.mass );
            #endregion
        }
        #endregion
    }
}