using System;
using System.Drawing;
using Microsoft.DirectX;

namespace Gas.Graphics
{
    public class LightNode : SceneGraphNode
    {
        #region Variables
        private Matrix localTransform = Matrix.Identity;
        private Light light = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the local transformation matrix.
        /// </summary>
        public Matrix LocalTransform
        {
            get
            {
                return localTransform;
            }
            set
            {
                localTransform = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of LightNode.
        /// </summary>
        public LightNode( Renderer renderer, SceneGraph sceneGraph )
            : base( renderer, sceneGraph )
        {
        }

        /// <summary>
        /// Initializes a new instance of LightNode.
        /// </summary>
        public LightNode( Renderer renderer, SceneGraph sceneGraph, Matrix localTransform,
            float range, float intensity, Color lightColor )
            : base( renderer, sceneGraph )
        {
            Vector2 pos = Vector2.TransformCoordinate( new Vector2(), localTransform );
            light = renderer.RegisterNewLight( range, intensity, pos, lightColor );
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates the light data, and updates all children nodes.
        /// </summary>
        public override void Update()
        {
            sceneGraph.MatrixStack.Push( localTransform );

            Vector2 pos = Vector2.TransformCoordinate( new Vector2(), sceneGraph.MatrixStack.CompositeTransform );
            light.Position = pos;
            base.Update();

            sceneGraph.MatrixStack.Pop();
        }
        #endregion
    }
}
