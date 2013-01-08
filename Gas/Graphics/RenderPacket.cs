using System;
using System.Collections.Generic;
using Microsoft.DirectX;

namespace Gas.Graphics
{
    /// <summary>
    /// A packet of information dispatched to the renderer. Used to separate rendering from the scene graph.
    /// </summary>
    public class RenderPacket
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of RenderPacket.
        /// </summary>
        public RenderPacket( IRenderable renderObject, string materialName, Matrix localTransform )
        {
            RenderObject = renderObject;
            MaterialName = materialName;
            LocalTransform = localTransform;
        }

        /// <summary>
        /// Initializes a new instance of RenderPacket.
        /// </summary>
        public RenderPacket( IRenderable renderObject, string materialName, Matrix localTransform,
            List<object> extraData )
        {
            RenderObject = renderObject;
            MaterialName = materialName;
            LocalTransform = localTransform;
            ExtraData = extraData;
        }
        #endregion

        #region Variables
        /// <summary>
        /// The IRenderable object to be rendered.
        /// </summary>
        public IRenderable RenderObject = null;

        /// <summary>
        /// The name of the material with which RenderObject is to be rendered.
        /// </summary>
        public string MaterialName = "NullMaterial";

        /// <summary>
        /// The local world transform for RenderObject.
        /// </summary>
        public Matrix LocalTransform = Matrix.Identity;

        /// <summary>
        /// A collection of generic-type data. This can be used to store information specific to individual
        /// instances of objects having the same material.
        /// </summary>
        public List<object> ExtraData = new List<object>();
        #endregion
    }
}