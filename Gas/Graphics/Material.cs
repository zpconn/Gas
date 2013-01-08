using System;
using System.Drawing;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// Represents a material. Stores the usual material information using Direct3D.Material, 
    /// as well as 8 textures for rendering and the name of the VisualEffect this material is 
    /// associated with.
    /// </summary>
    public class Material : IGraphicsResource
    {
        #region Variables
        public static readonly Color
            DefaultAmbientColor = Color.FromArgb( 40, 40, 40 ),
            DefaultDiffuseColor = Color.FromArgb( 210, 210, 210 ),
            DefaultSpecularColor = Color.FromArgb( 255, 255, 255 );

        public const float DefaultShininess = 24.0f;

        private Direct3D.Material d3dMaterial;

        private Texture[] textures = new Texture[ 8 ];

        private Renderer renderer = null;

        private string visualEffectName = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the internal Direct3D.Material object used by WarOfTheSeas.Graphics.Material.
        /// </summary>
        public Direct3D.Material D3DMaterial
        {
            get
            {
                return d3dMaterial;
            }
            set
            {
                d3dMaterial = value;
            }
        }

        /// <summary>
        /// Gets and sets the ambient color.
        /// </summary>
        public Color Ambient
        {
            get
            {
                return d3dMaterial.Ambient;
            }
            set
            {
                d3dMaterial.Ambient = value;
            }
        }

        /// <summary>
        /// Gets and sets the diffuse color.
        /// </summary>
        public Color Diffuse
        {
            get
            {
                return d3dMaterial.Diffuse;
            }
            set
            {
                d3dMaterial.Diffuse = value;
            }
        }

        /// <summary>
        /// Gets and sets the specular color.
        /// </summary>
        public Color Specular
        {
            get
            {
                return d3dMaterial.Specular;
            }
            set
            {
                d3dMaterial.Specular = value;
            }
        }

        /// <summary>
        /// Gets and sets the shininess (specular exponent).
        /// </summary>
        public float Shininess
        {
            get
            {
                return d3dMaterial.SpecularSharpness;
            }
            set
            {
                d3dMaterial.SpecularSharpness = value;
            }
        }

        /// <summary>
        /// Gets and sets the name of the VisualEffect with which this material will be rendered.
        /// </summary>
        public string VisualEffectName
        {
            get
            {
                return visualEffectName;
            }
            set
            {
                visualEffectName = value;
            }
        }

        /// <summary>
        /// Gets the texture list. This list stores up to 8 textures.
        /// </summary>
        public Texture[] Textures
        {
            get
            {
                return textures;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Builds the material using default parameters.
        /// </summary>
        public Material( Renderer renderer )
        {
            if ( renderer == null )
                throw new ArgumentNullException( "renderer",
                    "Cannot create a material with a null renderer reference." );

            this.renderer = renderer;

            d3dMaterial = new Direct3D.Material();
            d3dMaterial.Ambient = DefaultAmbientColor;
            d3dMaterial.Diffuse = DefaultDiffuseColor;
            d3dMaterial.Specular = DefaultSpecularColor;
            d3dMaterial.SpecularSharpness = DefaultShininess;
        }

        /// <summary>
        /// Builds the material using a standard Direct3D material structure and the name of the VisualEffect.
        /// </summary>
        public Material( Renderer renderer, Direct3D.Material material, string visualEffectName )
        {
            if ( renderer == null )
                throw new ArgumentNullException( "renderer",
                    "Cannot create a material with a null renderer reference." );

            this.renderer = renderer;
            d3dMaterial = material;
            this.visualEffectName = visualEffectName;
        }
        #endregion

        #region IGraphicsObject members
        public virtual void OnDeviceReset()
        {
        }

        public virtual void OnDeviceLost()
        {
        }

        public virtual void Dispose()
        {
        }
        #endregion

        #region Load from Xml file
        public static Material FromFile( Renderer renderer, string xmlFilename )
        {
            if ( renderer == null )
                throw new ArgumentNullException( "renderer",
                    "Cannot create a material with a null renderer reference." );

            if ( String.IsNullOrEmpty( xmlFilename ) )
                throw new ArgumentNullException( "xmlFilename",
                    "Cannot load a material without a valid filename." );

            if ( !File.Exists( xmlFilename ) )
                throw new FileNotFoundException( xmlFilename );

            Material material = new Material( renderer );
            XmlTextReader reader = new XmlTextReader( xmlFilename );

            while ( reader.Read() )
            {
                if ( reader.NodeType == XmlNodeType.Element )
                {
                    if ( reader.LocalName == "Ambient" )
                    {
                        int a = int.Parse( reader.GetAttribute( 0 ) );
                        int r = int.Parse( reader.GetAttribute( 1 ) );
                        int g = int.Parse( reader.GetAttribute( 2 ) );
                        int b = int.Parse( reader.GetAttribute( 3 ) );

                        material.Ambient = Color.FromArgb( a, r, g, b );
                    }
                    else if ( reader.LocalName == "Diffuse" )
                    {
                        int a = int.Parse( reader.GetAttribute( 0 ) );
                        int r = int.Parse( reader.GetAttribute( 1 ) );
                        int g = int.Parse( reader.GetAttribute( 2 ) );
                        int b = int.Parse( reader.GetAttribute( 3 ) );

                        material.Diffuse = Color.FromArgb( a, r, g, b );
                    }
                    else if ( reader.LocalName == "Specular" )
                    {
                        int a = int.Parse( reader.GetAttribute( 0 ) );
                        int r = int.Parse( reader.GetAttribute( 1 ) );
                        int g = int.Parse( reader.GetAttribute( 2 ) );
                        int b = int.Parse( reader.GetAttribute( 3 ) );

                        material.Specular = Color.FromArgb( a, r, g, b );
                    }
                    else if ( reader.LocalName == "Shininess" )
                    {
                        float shininess = float.Parse( reader.ReadString() );
                        material.Shininess = shininess;
                    }
                    else if ( reader.LocalName == "Texture" )
                    {
                        int index = int.Parse( reader.GetAttribute( 0 ) );
                        string filename = reader.GetAttribute( 1 );
                        material.Textures[ index ] = GlobalResourceCache.CreateTextureFromFile( renderer,
                            filename );
                    }
                    else if ( reader.LocalName == "VisualEffect" )
                    {
                        material.VisualEffectName = reader.ReadString();
                    }
                }
            }

            return material;
        }
        #endregion
    }
}