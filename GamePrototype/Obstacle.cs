using System;
using Microsoft.DirectX;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    class Obstacle : Entity
    {
        #region Variables
        private bool invisible = false;
        private string material = "";
        private Config config = new Config( "Config.txt" );
        private bool shadows = false;

        private static Mesh obstacleMesh = null;
        #endregion

        #region Properties
        public static Mesh ObstacleMesh
        {
            get
            {
                return obstacleMesh;
            }
            set
            {
                obstacleMesh = value;
            }
        }
        #endregion

        #region Constructor
        public Obstacle( Renderer renderer, bool invisible, string material )
            : base( renderer, EntityType.Obstacle )
        {
            this.invisible = invisible;
            this.material = material;
            this.mesh = obstacleMesh;

            mass = config.GetSetting<float>( "RobotMass" );
            shadows = config.GetSetting<bool>( "Shadows" );
        }

        public Obstacle( Renderer renderer, bool invisible, string material, Vector2 position )
            : base( renderer, EntityType.Obstacle )
        {
            this.invisible = invisible;
            this.material = material;
            this.mesh = obstacleMesh;
            this.position = position;

            mass = config.GetSetting<float>( "RobotMass" );
            shadows = config.GetSetting<bool>( "Shadows" );
        }
        #endregion

        #region Entity Methods
        public override void Update( float moveFactor )
        {
            base.Update( moveFactor );
        }

        public override void Render( float moveFactor )
        {
            if ( invisible )
                return;

            renderer.AddRenderPacket( new RenderPacket( mesh, material, worldTransform ) );

            if ( shadows )
            {
                for ( int i = 0; i < renderer.Lights.Count; ++i )
                {
                    ShadowManager.GenerateShadow( mesh, worldTransform, renderer.Lights[ i ].Position, -1.0f,
                        position );
                }
            }
        }

        public override void OnCollision( Entity hit )
        {
        }
        #endregion
    }
}
