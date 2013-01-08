using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
using NUnit.Framework;

namespace Gas.Helpers
{
    /// <summary>
    /// Contains static methods to ease working with strings.
    /// </summary>
    public class StringHelper
    {
        #region Constructor
        /// <summary>
        /// Private constructor to disallow instantiation.
        /// </summary>
        private StringHelper()
        {
        }
        #endregion

        #region Comparisons
        /// <summary>
        /// Performs a case insensitive comparison of two strings. For example, this method
        /// will deem "ABcD" as being equal to "abCd".
        /// </summary>
        /// <param name="s1">The first comparand.</param>
        /// <param name="s2">The second comparand.</param>
        /// <returns>A boolean value indicating whether the two strings were
        /// deemed equivalent or not.</returns>
        public static bool CaseInsensitiveCompare( string s1, string s2 )
        {
            return ( String.Equals( s1.ToLower(), s2.ToLower() ) );
        }
        #endregion

        #region Methods for dealing with filenames
        /// <summary>
        /// Extracts the extension (without the period) from a filename. For example,
        /// GetExtension("testImage.png") will return the string "png".
        /// </summary>
        /// <param name="filename">The filename from which to extract the extension.</param>
        /// <returns>A string representing the file extension, without the period.</returns>
        public static string GetExtension( string filename )
        {
            if ( String.IsNullOrEmpty( filename ) )
                return "";

            int dotIndex = filename.LastIndexOf( '.' );
            if ( dotIndex > 0 && dotIndex < filename.Length )
                return filename.Remove( 0, dotIndex + 1 );

            return "";
        }

        /// <summary>
        /// Removes the extension (including the period) from a filename. For example,
        /// RemoveExtension("testImage.png") will return the string "testImage".
        /// </summary>
        public static string RemoveExtension( string filename )
        {
            if ( String.IsNullOrEmpty( filename ) )
                return "";

            string ext = GetExtension( filename );
            if ( String.IsNullOrEmpty( ext ) )
                return filename;

            return filename.Remove( filename.Length - ext.Length - 1 );
        }

        /// <summary>
        /// Gets the name of a file without extension. For example, GetNameOfFile("Images\testImage.png")
        /// will return "testImage".
        /// </summary>
        public static string GetNameOfFile( string filename )
        {
            string filenameNoExt = RemoveExtension( filename );
            return filenameNoExt.Remove( 0, filenameNoExt.LastIndexOf( @"\" ) + 1 );
        }
        #endregion

        #region Misc. methods
        /// <summary>
        /// Returns true if a given string contains only whitespace. Returns false otherwise.
        /// </summary>
        public static bool ContainsOnlyWhitespace( string str )
        {
            foreach ( char ch in str )
            {
                if ( ch != ' ' )
                    return false;
            }

            return true;
        }
        #endregion
    }

    #region Unit testing
    [TestFixture]
    [Category( "Non-graphical" )]
    public class StringHelperTests
    {
        [Test]
        public void TestCaseInsensitiveCompare()
        {
            Assert.AreEqual( true, StringHelper.CaseInsensitiveCompare( "ABcD", "abCd" ) );
        }

        [Test]
        public void TestGetExtension()
        {
            Assert.AreEqual( "png", StringHelper.GetExtension( "testImage.png" ) );
        }
    }
    #endregion
}