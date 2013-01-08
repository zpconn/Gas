using System;
using System.Collections.Generic;
using Microsoft.DirectX;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// Represents complicated transformations by storing them as the concatenation of many submatrices
    /// stored in a stack structure. The organizational structure and dynamics of the internal stack allow
    /// spatial relationships between many objects to be stored in a convenient and computationally 
    /// inexpensive manner.
    /// 
    /// The Nth matrix in the stack is the product of all matrices preceding it and another, external matrix.
    /// </summary>
    public class MatrixStack
    {
        #region Variables
        private Stack<Matrix> matrixStack = new Stack<Matrix>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the composite local matrix transform represented by the matrix stack. This is the product 
        /// of all the matrices in the stack, multiplied from top to bottom.
        /// </summary>
        public Matrix CompositeTransform
        {
            get
            {
                if ( matrixStack.Count > 0 )
                    return matrixStack.Peek();
                else
                    return Matrix.Identity;
            }
        }
        #endregion

        #region Stack manipulation methods
        /// <summary>
        /// Concatenates a matrix with the composite local transform represented by the stack, and pushes
        /// the result onto the top of the stack.
        /// </summary>
        public void Push( Matrix localTransform )
        {
            if ( matrixStack.Count > 0 )
                matrixStack.Push( localTransform * CompositeTransform );
            else
                matrixStack.Push( localTransform );
        }

        /// <summary>
        /// Pops the topmost matrix off of the stack.
        /// </summary>
        public void Pop()
        {
            matrixStack.Pop();
        }
        #endregion
    }

    /// <summary>
    /// Represents a scene graph: an object-oriented structure for storing and working with
    /// relationships between scene objects. The effects of parent nodes are propagated down to
    /// their children, so that as the tree is traversed effects are accumulated. Thus, scene data
    /// can be arranged in a very hierarchical fashion, allowing efficient and intuitive storage of complicated
    /// spatial as well as other relationships.
    /// </summary>
    public class SceneGraph
    {
        #region Variables
        private Renderer renderer = null;
        private SceneGraphNode root = null;

        private MatrixStack matrixStack = new MatrixStack();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the internal matrix stack, used for accumulating local matrix transforms
        /// as the scene graph is traversed.
        /// </summary>
        public MatrixStack MatrixStack
        {
            get
            {
                return matrixStack;
            }
        }

        /// <summary>
        /// Gets the root node of the scene graph.
        /// </summary>
        public SceneGraphNode Root
        {
            get
            {
                return root;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of SceneGraph.
        /// </summary>
        public SceneGraph( Renderer renderer )
        {
            if ( renderer == null )
            {
                Log.Write( "Can't create a SceneGraph with a null Renderer reference." );
                throw new ArgumentNullException( "renderer",
                    "Can't create a SceneGraph with a null Renderer reference." );
            }

            this.renderer = renderer;

            root = new SceneGraphNode( this.renderer, this );
        }
        #endregion

        #region Update
        /// <summary>
        /// Traverses the graph recursively, updating all the nodes.
        /// </summary>
        public void Update()
        {
            if ( root == null )
            {
                Log.Write( "The root node in the scene graph is null. Will not update!" );
                return;
            }

            root.Update();
        }
        #endregion
    }
}