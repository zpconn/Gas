using System;
using System.Drawing;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    class ExplodingProjectile : Projectile
    {
        #region Variables
        private Scene scene = null;
        private int clusterSize = 0;

        private Light light = null;
        #endregion

        #region Constructor
        public ExplodingProjectile( Renderer renderer, Vector2 startPos, Vector2 direction,
            Vector2 initialVelocity, Scene scene )
            : base( renderer, startPos, direction, initialVelocity )
        {
            lifetime = config.GetSetting<float>( "ExplodingProjectileTimer" );
            clusterSize = config.GetSetting<int>( "ExplodingProjectileClusterSize" );
            projectileDamage = config.GetSetting<float>( "ExplodingProjectileDamage" );
            speed = config.GetSetting<float>( "ExplodingProjectileSpeed" );

            mesh = renderer.CreateCircularMesh(Color.Black, 13.0f, 6 );

            light = renderer.RegisterNewLight( 200.0f, 1.0f, startPos, Color.Red );

            this.velocity = initialVelocity + direction * speed;

            this.scene = scene;
        }
        #endregion

        #region Entity methods
        public override void Update( float moveFactor )
        {
            base.Update( moveFactor );

            // We can't use the 'alive' variable to perform this check, because 'alive' is set to false
            // upon colliding with a robot. The robot would be bombarded with the entire cluster!
            if ( age >= lifetime )
                ExplodeCluster();

            light.Position = position;
        }

        public override void Render( float moveFactor )
        {
            renderer.AddRenderPacket( new RenderPacket( mesh, "explodingProjectile", worldTransform ) );
        }

        private void ExplodeCluster()
        {
            float angleStep = ( 2.0f * ( float )Math.PI ) / ( float )clusterSize;
            for ( int i = 0; i < clusterSize; ++i )
            {
                float angle = angleStep * i;
                Vector2 direction = new Vector2( ( float )Math.Cos( angle ), ( float )Math.Sin( angle ) );

                // We must set the initial position slightly away from the position of this exploding projectile.
                // Otherwise, all the bullets in the explosion cluster will collide with one another instantly, causing
                // chaos. A systematic way to accomplish this is to slide the projectile's initial position along 
                // its direction vector computed above.

                Vector2 initialPos = position + 20.0f * direction;

                scene.AddEntity( new Projectile( renderer, initialPos, direction, velocity ) );
            }
        }

        public override void Kill()
        {
            base.Kill();

            renderer.RemoveLight( light );
        }
        #endregion
    }
}