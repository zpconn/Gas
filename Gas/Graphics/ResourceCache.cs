using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Gas.Helpers;

namespace Gas.Graphics
{
    /// <summary>
    /// Stores information about a cached font.
    /// </summary>
    public struct CachedFont
    {
        public string FamilyName;
        public int Height;
    }

    /// <summary>
    /// Caches resources as they are created, so that a single resource can be shared among objects
    /// without being duplicated in memory.
    /// </summary>
    public class GlobalResourceCache
    {
        #region Variables
        private static Hashtable textureCache = new Hashtable();
        private static Hashtable fontCache = new Hashtable();
        private static Hashtable effectCache = new Hashtable();
        private static Hashtable materialCache = new Hashtable();
        #endregion

        #region Methods for creating resources
        /// <summary>
        /// Create a texture from a file. If the texture has already been created, the cached
        /// version is returned.
        /// </summary>
        public static Texture CreateTextureFromFile( Renderer renderer, string filename )
        {
            // Search cache first
            foreach ( string cachedFilename in textureCache.Keys )
            {
                if ( StringHelper.CaseInsensitiveCompare( cachedFilename, filename ) )
                    return textureCache[ cachedFilename ] as Texture;
            }

            Texture newTex = new Texture( renderer, filename );

            textureCache.Add( filename, newTex );

            return newTex;
        }

        /// <summary>
        /// Creates a font. If the font has already been created, the cached version is returned.
        /// </summary>
        public static Font CreateFont( Renderer renderer, string familyName, int height )
        {
            // Search cache first
            foreach ( CachedFont cf in fontCache.Keys )
            {
                if ( StringHelper.CaseInsensitiveCompare( cf.FamilyName, familyName ) &&
                    cf.Height == height )
                    return fontCache[ cf ] as Font;
            }

            Font newFont = new Font( renderer, familyName, height );
            CachedFont cachedFont = new CachedFont();
            cachedFont.FamilyName = familyName;
            cachedFont.Height = height;

            fontCache.Add( cachedFont, newFont );

            return newFont;
        }

        /// <summary>
        /// Creates an effect from a file. If the effect has already been created, the cached version
        /// is returned.
        /// </summary>
        public static Effect CreateEffectFromFile( Renderer renderer, string filename )
        {
            // Search cache first
            foreach ( string cachedFilename in effectCache.Keys )
            {
                if ( StringHelper.CaseInsensitiveCompare( cachedFilename, filename ) )
                    return effectCache[ cachedFilename ] as Effect;
            }

            Effect newEffect = new Effect( renderer, filename );

            effectCache.Add( filename, newEffect );

            return newEffect;
        }

        /// <summary>
        /// Creates a material from a file. If the material has already been created, the cached version
        /// is returned.
        /// </summary>
        public static Material CreateMaterialFromFile( Renderer renderer, string filename )
        {
            // Search cache first
            foreach ( string cachedFilename in materialCache.Keys )
            {
                if ( StringHelper.CaseInsensitiveCompare( cachedFilename, filename ) )
                    return materialCache[ cachedFilename ] as Material;
            }

            Material newMat = Material.FromFile( renderer, filename );

            materialCache.Add( filename, newMat );

            return newMat;
        }
        #endregion
    }
}