using System;
using System.Collections.Generic;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// A node in the scene graph. Keeps a list of its children, and provides functionality for updating
    /// the subtree of the scene graph with this node as its root.
    /// </summary>
    public class SceneGraphNode : IDisposable
    {
        #region Variables
        protected Renderer renderer = null;
        protected SceneGraph sceneGraph = null;
        protected List<SceneGraphNode> children = new List<SceneGraphNode>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of SceneGraphNode.
        /// </summary>
        public SceneGraphNode( Renderer renderer, SceneGraph sceneGraph )
        {
            if ( renderer == null )
            {
                Log.Write( "Cannot create a SceneGraphNode with a null Renderer reference." );
                throw new ArgumentNullException( "renderer",
                    "Cannot create a SceneGraphNode with a null Renderer reference." );
            }

            if ( sceneGraph == null )
            {
                Log.Write( "Cannot create a SceneGraphNode with a null SceneGraph reference." );
                throw new ArgumentNullException( "sceneGraph",
                    "Cannot create a SceneGraphNode with a null SceneGraph reference." );
            }

            this.renderer = renderer;
            this.sceneGraph = sceneGraph;
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Updates all the children of this node in the scene graph from left-to-right.
        /// </summary>
        public virtual void Update()
        {
            foreach ( SceneGraphNode child in children )
            {
                child.Update();
            }
        }

        /// <summary>
        /// Adds a child to this node in the scene graph.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild( SceneGraphNode child )
        {
            children.Add( child );
        }

        /// <summary>
        /// Frees all the resources controlled by this SceneGraphNode.
        /// </summary>
        public void Dispose()
        {
            children.Clear();
        }
        #endregion
    }
}