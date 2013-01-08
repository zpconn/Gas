using System;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace Gas.Input
{
    /// <summary>
    /// Helper class to acquire statistics about a keyboard device.
    /// </summary>
    public class KeyboardDevice
    {
        #region Variables
        /// <summary>
        /// The DirectInput device representing the keyboard.
        /// </summary>
        private Device keyboard = null;

        /// <summary>
        /// Statistics detailing the current state of the keyboard (such as which keys are pressed).
        /// </summary>
        private KeyboardState state;
        #endregion

        #region Properties
        /// <summary>
        /// Is the left arrow key pressed?
        /// </summary>
        public bool Left
        {
            get
            {
                return ( state != null ) && state[ Key.LeftArrow ];
            }
        }

        /// <summary>
        /// Is the right arrow key pressed?
        /// </summary>
        public bool Right
        {
            get
            {
                return ( state != null ) && state[ Key.RightArrow ];
            }
        }

        /// <summary>
        /// Is the up arrow key pressed?
        /// </summary>
        public bool Up
        {
            get
            {
                return ( state != null ) && state[ Key.UpArrow ];
            }
        }

        /// <summary>
        /// Is the down arrow key pressed?
        /// </summary>
        public bool Down
        {
            get
            {
                return ( state != null ) && state[ Key.DownArrow ];
            }
        }

        /// <summary>
        /// Is the W key pressed?
        /// </summary>
        public bool W
        {
            get
            {
                return ( state != null ) && state[ Key.W ];
            }
        }

        /// <summary>
        /// Is the A key pressed?
        /// </summary>
        public bool A
        {
            get
            {
                return ( state != null ) && state[ Key.A ];
            }
        }

        /// <summary>
        /// Is the S key pressed?
        /// </summary>
        public bool S
        {
            get
            {
                return ( state != null ) && state[ Key.S ];
            }
        }

        /// <summary>
        /// Is the D key pressed?
        /// </summary>
        public bool D
        {
            get
            {
                return ( state != null ) && state[ Key.D ];
            }
        }

        /// <summary>
        /// Is the Escape key pressed?
        /// </summary>
        public bool Escape
        {
            get
            {
                return ( state != null ) && state[ Key.Escape ];
            }
        }

        /// <summary>
        /// Returns the KeyboardState indicating whether each key is pressed or not.
        /// </summary>
        public KeyboardState State
        {
            get
            {
                return state;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of KeyboardDevice.
        /// </summary>
        public KeyboardDevice( Form form )
        {
            keyboard = new Device( SystemGuid.Keyboard );

            keyboard.SetCooperativeLevel( form, CooperativeLevelFlags.Background |
                CooperativeLevelFlags.NonExclusive );

            keyboard.Acquire();
        }
        #endregion

        #region Update
        /// <summary>
        /// Acquires updated keyboard state statistics.
        /// </summary>
        public void Update()
        {
            state = keyboard.GetCurrentKeyboardState();
        }
        #endregion
    }
}