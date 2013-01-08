using System;
using System.Drawing;
using System.IO;
using System.Collections;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// Encapsulates a Direct3D.Effect object. Provides a simplified interface, performs
    /// extensive error checking, caches effect parameter handles, and provides some additional 
    /// functionality. This class is used for all the shader effects in the game.
    /// 
    /// Assumes the existence of a default technique named "DefaultTechnique".
    /// </summary>
    public class Effect : IGraphicsResource
    {
        #region Variables
        private Direct3D.Effect effect = null;
        private Hashtable effectHandles = new Hashtable();
        private bool insidePass = false;
        #endregion

        #region Construction from a file
        public Effect( Renderer renderer, string filename )
        {
            //if (!File.Exists(filename))
            //    throw new FileNotFoundException(filename);

            if ( renderer == null )
                throw new ArgumentNullException( "renderer",
                    "Can't create an Effect without a valid renderer." );

            string compilationErrors = "";

            try
            {
                effect = Direct3D.Effect.FromFile( renderer.Device, filename, null, null,
                    null, ShaderFlags.None, null, out compilationErrors );

            }
            catch
            {
                Log.Write( "Unable to create effect " + filename + ". The compilation errors were: \n\n" +
                    compilationErrors );
            }

            renderer.AddGraphicsObject( this );
        }
        #endregion

        #region Effect control methods
        /// <summary>
        /// Propagates the state change that occurs inside of an active pass to the device before rendering.
        /// </summary>
        public void CommitChanges()
        {
            effect.CommitChanges();
        }

        public void SetValue( string name, BaseTexture val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, Texture val )
        {
            effect.SetValue( GetHandle( name ), val.D3DTexture );
        }

        public void SetValue( string name, bool val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, bool[] val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, ColorValue val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, ColorValue[] val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, GraphicsStream val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, int val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, int[] val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, Matrix val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, Matrix[] val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, float val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, float[] val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, string val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, Vector4 val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public void SetValue( string name, Vector4[] val )
        {
            effect.SetValue( GetHandle( name ), val );
        }

        public unsafe void SetValue( string name, void* val, int len )
        {
            effect.SetValue( GetHandle( name ), val, len );
        }

        private EffectHandle GetHandle( string name )
        {
            if ( !effectHandles.ContainsKey( name ) )
            {
                effectHandles.Add( name, effect.GetParameter( null, name ) );
            }

            return effectHandles[ name ] as EffectHandle;
        }

        /// <summary>
        /// Begins the technique named "DefaultTechnique".
        /// </summary>
        public void BeginTechnique()
        {
            try
            {
                effect.Technique = "DefaultTechnique";
                effect.Begin( 0 );
            }
            catch
            {
                throw new DirectXException( "Effect.BeginTechnique() failed." );
            }
        }

        /// <summary>
        /// Ends the current pass if there is one, and begins the next one.
        /// </summary>
        public void Pass( int passNumber )
        {
            try
            {
                if ( insidePass )
                    effect.EndPass();

                effect.BeginPass( passNumber );
                insidePass = true;
            }
            catch
            {
                throw new DirectXException( "Effect.Pass() failed." );
            }
        }

        /// <summary>
        /// Ends the current technique.
        /// </summary>
        public void EndTechnique()
        {
            try
            {
                if ( insidePass )
                {
                    effect.EndPass();
                    insidePass = false;
                }

                effect.End();
            }
            catch
            {
                throw new DirectXException( "Effect.EndTechnique() failed." );
            }
        }

        #endregion

        #region IGraphicsObject members
        public void OnDeviceReset()
        {
            if ( effect != null )
                effect.OnResetDevice();
        }

        public void OnDeviceLost()
        {
            if ( effect != null )
                effect.OnLostDevice();
        }

        public void Dispose()
        {
            if ( effect != null )
                effect.Dispose();

            effect = null;
        }
        #endregion
    }
}