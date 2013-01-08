using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using NUnit.Framework;

namespace Gas.Helpers
{
    /// <summary>
    /// A utility class to help with dealing with enumerations. It provides functionality
    /// for treating enums as if they were collections.
    /// </summary>
    class EnumHelper
    {
        #region The Enum enumerator
        /// <summary>
        /// Provides functionality for enumerating enums as if they were collections.
        /// </summary>
        public class EnumEnumerator : IEnumerator, IEnumerable
        {
            #region Variables
            /// <summary>
            /// The System.Type of the enum we are enumerating.
            /// </summary>
            public System.Type enumType;

            /// <summary>
            /// The index of the enum element we're currently at.
            /// </summary>
            public int currentIndex;

            /// <summary>
            /// The number of elements in the enum.
            /// </summary>
            public int enumLength;
            #endregion

            /// <summary>
            /// Constructor: creates the enumerator.
            /// </summary>
            public EnumEnumerator( System.Type enumType )
            {
                this.enumType = enumType;
                currentIndex = -1;
                enumLength = GetSize( enumType );
            }

            /// <summary>
            /// Gets the element of the enum we're currently at.
            /// </summary>
            public object Current
            {
                get
                {
                    if ( currentIndex >= 0 && currentIndex < enumLength )
                    {
                        return Enum.GetValues( enumType ).GetValue( currentIndex );
                    }
                    else
                    {
                        // The current index is invalid; just return the first element.
                        return Enum.GetValues( enumType ).GetValue( 0 );
                    }
                }
            }

            /// <summary>
            /// Moves the enumerator to the next element in the enum.
            /// </summary>
            /// <returns>A boolean: false indicates we're done enumerating, true indicates
            /// we still have some elements to go.</returns>
            public bool MoveNext()
            {
                ++currentIndex;
                return currentIndex < enumLength;
            }

            /// <summary>
            /// Resets the enumerator to the first element in the enum.
            /// </summary>
            public void Reset()
            {
                currentIndex = -1;
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            public IEnumerator GetEnumerator()
            {
                return this;
            }
        }
        #endregion

        #region General methods
        /// <summary>
        /// Private constructor to disallow instantiation.
        /// </summary>
        private EnumHelper()
        {
        }

        /// <summary>
        /// Gets the number of elements in an enum.
        /// </summary>
        public static int GetSize( System.Type enumType )
        {
            return Enum.GetNames( enumType ).Length;
        }

        /// <summary>
        /// Creates an EnumEnumerator for the passed in enum type.
        /// </summary>
        public static EnumEnumerator GetEnumerator( System.Type enumType )
        {
            return new EnumEnumerator( enumType );
        }

        /// <summary>
        /// Searches an enum of type enumType for the element named elementName.
        /// </summary>
        /// <returns>A System.Object representing the desired enum element.</returns>
        public static object SearchEnum( System.Type enumType, string elementName )
        {
            // Search for an element named elementName.
            foreach ( object element in GetEnumerator( enumType ) )
            {
                if ( StringHelper.CaseInsensitiveCompare( element.ToString(), elementName ) )
                    return element;
            }

            // No valid element was found; return null.
            return null;
        }
        #endregion
    }

    #region Unit testing
    [TestFixture]
    [Category( "Non-graphical" )]
    public class EnumHelperTests
    {
        public enum TestEnum
        {
            Bob,
            Explosion,
            MightAndMagic
        }

        [Test]
        public void TestGetSize()
        {
            Assert.AreEqual( 3, EnumHelper.GetSize( typeof( TestEnum ) ) );
        }

        [Test]
        public void TestSearchEnum()
        {
            Assert.AreNotEqual( EnumHelper.SearchEnum( typeof( TestEnum ), "Bob" ), null );
            Assert.AreEqual( EnumHelper.SearchEnum( typeof( TestEnum ), "explosion" ).ToString(), "Explosion" );
        }

        [Test]
        public void TestEnumEnumerator()
        {
            List<string> elementNameList = new List<string>();
            foreach ( Enum enumValue in EnumHelper.GetEnumerator( typeof( TestEnum ) ) )
                elementNameList.Add( enumValue.ToString() );

            Assert.AreEqual( "Bob", elementNameList[ 0 ] );
            Assert.AreEqual( "Explosion", elementNameList[ 1 ] );
            Assert.AreEqual( "MightAndMagic", elementNameList[ 2 ] );
        }
    }
    #endregion
}