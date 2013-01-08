using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;
using Gas.Graphics;
using Gas.Helpers;
using Gas.Input;
using Graphics = Gas.Graphics;

namespace Gas.Graphics
{
    /// <summary>
    /// Represents a point light positioned in the XY plane.
    /// </summary>
    public class Light
    {
        #region Variables
        /// <summary>
        /// A reference to the renderer.
        /// </summary>
        private Renderer renderer = null;

        /// <summary>
        /// The light's position in the XY plane.
        /// </summary>
        private Vector2 position = new Vector2();

        /// <summary>
        /// The range of the light. This represents the maximum distance an object can be from this light
        /// and still be lit by it.
        /// </summary>
        private float range = 0.0f;

        /// <summary>
        /// The color of light emmitted by this light source.
        /// </summary>
        private Color color = Color.White;

        /// <summary>
        /// A value in the range [0, 1] representing the intensity of this light.
        /// </summary>
        private float intensity = 1.0f;
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the light's position.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Gets and sets the light color.
        /// </summary>
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        /// <summary>
        /// Gets and sets the range of the light. This represents the maximum distance 
        /// an object can be from this light and still be lit by it. 
        /// </summary>
        public float Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
            }
        }

        /// <summary>
        /// Gets and sets the range of the light. This represents the maximum distance 
        /// an object can be from this light and still be lit by it.
        /// </summary>
        public float Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
            }
        }
        #endregion

        #region Constructor
        public Light( Renderer renderer, float range, float intensity )
        {
            if ( renderer == null )
            {
                Log.Write( "'renderer' is null." );
                throw new ArgumentNullException( "renderer", "Can't create a Light with a null " +
                    "renderer reference." );
            }

            if ( intensity < 0.0f || intensity > 1.0f )
            {
                Log.Write( "'intensity' is out of range." );
                throw new ArgumentOutOfRangeException( "intensity", intensity,
                    "'intensity' is outside of the range [0,1]." );
            }

            this.renderer = renderer;
            this.range = range;
            this.intensity = intensity;
        }

        public Light( Renderer renderer, float range, float intensity, Vector2 position, Color color )
        {
            if ( renderer == null )
            {
                Log.Write( "'renderer' is null." );
                throw new ArgumentNullException( "renderer", "Can't create a Light with a null " +
                    "renderer reference." );
            }

            if ( intensity < 0.0f || intensity > 1.0f )
            {
                Log.Write( "'intensity' is out of range." );
                throw new ArgumentOutOfRangeException( "intensity", intensity,
                    "'intensity' is outside of the range [0,1]." );
            }

            this.renderer = renderer;
            this.range = range;
            this.intensity = intensity;
            this.position = position;
            this.color = color;
        }
        #endregion
    }
}