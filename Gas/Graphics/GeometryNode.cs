using System;
using Microsoft.DirectX;

namespace Gas.Graphics
{
    /// <summary>
    /// Represents a local transform in the scene graph hierarchy.
    /// </summary>
    public class GeometryNode : SceneGraphNode
    {
        #region Variables
        private Matrix localTransform = Matrix.Identity;
        private IRenderable renderObject = null;
        private string material = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the local transform matrix.
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
        /// Initializes a new instance of GeometryNode.
        /// </summary>
        public GeometryNode( Renderer renderer, SceneGraph sceneGraph )
            : base( renderer, sceneGraph )
        {
        }

        /// <summary>
        /// Initializes a new instance of GeometryNode.
        /// </summary>
        public GeometryNode( Renderer renderer, SceneGraph sceneGraph, Matrix localTransform,
            IRenderable renderObject, string material )
            : base( renderer, sceneGraph )
        {
            this.localTransform = localTransform;
            this.renderObject = renderObject;
            this.material = material;
        }
        #endregion

        #region Update
        /// <summary>
        /// Pushes the local transform onto the matrix stack, renders this node, 
        /// updates all the children of this node in the scene graph, and then 
        /// pops this local transform.
        /// </summary>
        public override void Update()
        {
            sceneGraph.MatrixStack.Push( localTransform );

            // Dispatch rendering off to the Renderer and outside of the scene graph
            renderer.AddRenderPacket( new RenderPacket( renderObject, material,
                sceneGraph.MatrixStack.CompositeTransform ) );

            base.Update();

            sceneGraph.MatrixStack.Pop();
        }
        #endregion
    }
}