using System;
using System.Drawing;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    class Projectile : Entity
    {
        #region Variables
        protected Config config = new Config( "Config.txt" );
        protected float lifetime = 0.0f;
        protected float age = 0.0f;
        protected Vector2 direction = new Vector2();
        protected Vector2 initialVelocity = new Vector2();
        protected float speed = 0.0f;
        protected float projectileDamage = 0.0f;
        #endregion

        #region Constructor
        public Projectile( Renderer renderer, Vector2 startPos, Vector2 direction,
            Vector2 initialVelocity )
            : base( renderer, EntityType.Projectile )
        {
            this.renderer = renderer;
            this.Position = startPos;
            this.direction = direction;
            this.initialVelocity = initialVelocity;

            lifetime = config.GetSetting<float>( "ProjectileLifetime" );
            speed = config.GetSetting<float>( "ProjectileSpeed" );
            projectileDamage = config.GetSetting<float>( "ProjectileDamage" );

            //light = renderer.RegisterNewLight(250.0f, 1.0f, this.Position, Color.Green);

            mesh = Mesh.Circle( renderer, Color.Black, 5.0f, 6 );

            this.velocity = initialVelocity + direction * speed;
        }
        #endregion

        #region Entity Methods
        public override void Update( float moveFactor )
        {
            age += moveFactor;

            if ( age >= lifetime )
                this.alive = false;

            base.Update( moveFactor );
        }

        public override void Render( float moveFactor )
        {
            renderer.AddRenderPacket( new RenderPacket( mesh, "projectile", worldTransform ) );
        }

        public override void OnCollision( Entity hit )
        {
            if ( hit.Type == EntityType.Obstacle )
            {
                Obstacle obstacle = hit as Obstacle;

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

                // Reflect velocity vector over collision normal
                Vector2 normal = Vector2.Normalize( this.position - obstacle.Position );
                this.velocity -= 2 * normal * ( Vector2.Dot( velocity, normal ) );
            }
            else if ( hit.Type == EntityType.Robot )
            {
                Robot bot = hit as Robot;
                bot.ApplyDamage( ( int )projectileDamage );

                this.alive = false;
            }
            else if ( hit.Type == EntityType.Projectile )
            {
                Projectile projectile = hit as Projectile;

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

                // Reflect velocity vector over collision normal
                Vector2 normal = Vector2.Normalize( this.position - projectile.Position );
                this.velocity -= 2 * normal * ( Vector2.Dot( velocity, normal ) );
            }
        }

        public override void Kill()
        {
        }
        #endregion
    }
}
