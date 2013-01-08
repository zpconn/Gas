using System;
using Microsoft.DirectX;
using Gas.Graphics;

namespace GamePrototype
{
    public enum EntityType
    {
        Generic = 0,
        Robot,
        Projectile,
        Obstacle
    }

    public class Entity
    {
        #region Variables
        protected Renderer renderer = null;
        protected Mesh mesh = null;
        protected EntityType type = EntityType.Generic;
        protected Vector2 position = new Vector2();
        protected Vector2 velocity = new Vector2();
        protected Vector2 acceleration = new Vector2();
        protected float orientation = 0.0f;
        protected float angularVelocity = 0.0f;
        protected Matrix worldTransform = Matrix.Identity;
        protected bool alive = true;
        protected float mass = 0.0f;
        #endregion

        #region Properties
        public bool Alive
        {
            get
            {
                return alive;
            }
        }

        public Mesh Mesh
        {
            get
            {
                return mesh;
            }
        }

        public float Mass
        {
            get
            {
                return mass;
            }
        }

        public EntityType Type
        {
            get
            {
                return type;
            }
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                worldTransform = Matrix.RotationZ( orientation ) * Matrix.Translation( position.X, position.Y, 0.0f );
            }
        }

        public Vector2 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
            }
        }

        public Vector2 Acceleration
        {
            get
            {
                return acceleration;
            }
            set
            {
                acceleration = value;
            }
        }

        public float Orientation
        {
            get
            {
                return orientation;
            }
        }

        public float AngularVelocity
        {
            get
            {
                return angularVelocity;
            }
        }

        public Matrix WorldTransform
        {
            get
            {
                return worldTransform;
            }
        }
        #endregion

        #region Public Interface
        public virtual void Update( float moveFactor )
        {
            // Integrate equations of motion using the Euler scheme

            velocity += acceleration * moveFactor;
            position += velocity * moveFactor;
            orientation += angularVelocity * moveFactor;

            // Normalize the orientation to the range [0,2Pi)
            orientation %= 2.0f * ( float )Math.PI;

            // Build world transform matrix
            worldTransform = Matrix.RotationZ( orientation ) * Matrix.Translation( position.X, position.Y, 0.0f );
        }

        public virtual void Render( float moveFactor )
        {
        }

        public virtual void OnCollision( Entity hit ) { }

        public virtual void Kill() { }
        #endregion

        #region Constructor
        public Entity( Renderer renderer, EntityType type )
        {
            this.renderer = renderer;
            this.type = type;
        }
        #endregion
    }
}
