using System;
using Microsoft.DirectX;
using Gas.Graphics;

namespace GamePrototype
{
    public class Camera
    {
        #region Variables
        private Renderer renderer = null;

        private Vector2 position = new Vector2();
        private float depth = -5.0f;
        #endregion

        #region Properties
        public Vector2 FocusPoint
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                BuildViewMatrix();
            }
        }

        public float Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
                BuildViewMatrix();
            }
        }
        #endregion

        #region Constructor
        public Camera( Renderer renderer )
        {
            this.renderer = renderer;
        }
        #endregion

        #region Public Interface
        public void Slide( Vector2 slideVector )
        {
            position += slideVector;
            BuildViewMatrix();
        }
        #endregion

        #region BuildViewMatrix
        private void BuildViewMatrix()
        {
            renderer.ViewMatrix = Matrix.LookAtLH( new Vector3( position.X, position.Y, depth ),
                new Vector3( position.X, position.Y, 0.0f ), new Vector3( 0, 1.0f, 0 ) );
        }
        #endregion
    }
}
