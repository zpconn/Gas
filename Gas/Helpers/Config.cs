using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Gas.Helpers
{
    /// <summary>
    /// Loads and stores configuration/settings data from a config file.
    /// </summary>
    public class Config
    {
        #region Variables
        /// <summary>
        /// Stores the settings as strings, and maps them to names.
        /// </summary>
        private Dictionary<string, string> config = new Dictionary<string, string>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of Config, and loads configuration/settings data
        /// from the config file specified by the parameter 'filename'.
        /// </summary>
        /// <param name="filename"></param>
        public Config( string filename )
        {
            ReloadFromFile( filename );
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a setting according to its key, and casts it to type T. T must have a method called
        /// "Parse" if it is not of type System.String.
        /// </summary>
        public void GetSetting<T>( out T setting, string key )
        {
            if ( typeof( T ) == typeof( string ) )
            {
                setting = ( T )( ( object )config[ key ] );
                return;
            }

            try
            {
                setting = ( T )typeof( T ).GetMethod( "Parse", new Type[] { typeof( String ) } ).Invoke( null,
                    new object[] { config[ key ] } );
            }
            catch ( Exception ex )
            {
                Log.Write( "Unable to read setting " + key + " and cast it to type " +
                    typeof( T ).ToString() );
                throw ex;
            }
        }

        /// <summary>
        /// Gets a setting according to its key, and casts it to type T. T must have a method called
        /// "Parse".
        /// </summary>
        public T GetSetting<T>( string key )
        {
            T setting;
            GetSetting( out setting, key );

            return setting;
        }

        /// <summary>
        /// Clears the existing setting cache, parses the new settings file, and caches the data
        /// for lookup later on.
        /// </summary>
        public void ReloadFromFile( string filename )
        {
            if ( string.IsNullOrEmpty( filename ) )
            {
                Log.Write( "Invalid filename passed to Config.ReloadFromFile()." );
                throw new ArgumentNullException( "filename", "Invalid filename passed to " +
                    "Config.ReloadFromFile()." );
            }

            if ( !File.Exists( filename ) )
            {
                Log.Write( "Unable to find config file. Attempted to load: " + filename );
                throw new FileNotFoundException( "Unable to find config file.", filename );
            }

            if ( config.Count > 0 )
                config.Clear();

            string[] fileLines = File.ReadAllLines( filename );

            foreach ( string line in fileLines )
            {
                // Remove whitespace
                string trimmedLine = line.Trim();

                if ( trimmedLine.StartsWith( "//" ) || string.IsNullOrEmpty( trimmedLine ) ||
                    StringHelper.ContainsOnlyWhitespace( trimmedLine ) )
                    continue;

                // Get rid of any comments that might be at the end of the trimmedLine
                if ( trimmedLine.Contains( "//" ) )
                    trimmedLine = trimmedLine.Remove( trimmedLine.IndexOf( "//" ) );

                int colonIndex = trimmedLine.IndexOf( ":" );

                string key = trimmedLine.Substring( 0, colonIndex );
                string value = trimmedLine.Substring( colonIndex + 1 ).Trim();

                // Cache the setting and associate it with its name
                config[ key ] = value;
            }
        }
        #endregion
    }
}