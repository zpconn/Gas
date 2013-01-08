using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using NUnit.Framework;
using Microsoft.DirectX.DirectInput;
using DirectInput = Microsoft.DirectX.DirectInput;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Gas.Graphics;

namespace Gas.Input
{
    /// <summary>
    /// Helper class to acquire statistics about a mouse device.
    /// </summary>
    public class MouseDevice
    {
        #region Variables
        /// <summary>
        /// The DirectInput device representing the mouse.
        /// </summary>
        private DirectInput.Device mouseDevice = null;

        /// <summary>
        /// The mouse movement vector. The z-component is mapped to the mouse wheel, if it
        /// is present.
        /// </summary>
        private Vector3 movementVector = new Vector3();

        /// <summary>
        /// Are the left or right mouse buttons pressed?
        /// </summary>
        private bool leftButtonPressed = false, rightButtonPressed = false;

        /// <summary>
        /// A number in the range [0,1]. The greater is this number, the greater is the motion of the mouse
        /// smoothed. 0 indicates no smoothing at all.
        /// </summary>
        private float smoothingFactor = 0.25f;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the movement vector. The z-component is mapped to the mouse wheel if it is
        /// present.
        /// </summary>
        public Vector3 MovementVector
        {
            get
            {
                return -movementVector;
            }
        }

        /// <summary>
        /// Gets the x-delta, representing how far the mouse has moved along the x-axis.
        /// </summary>
        public float XDelta
        {
            get
            {
                return -movementVector.X;
            }
        }

        /// <summary>
        /// Gets the y-delta, representing how far the mouse has moved along the y-axis.
        /// </summary>
        public float YDelta
        {
            get
            {
                return -movementVector.Y;
            }
        }

        /// <summary>
        /// Gets the wheel delta, representing how far the wheel was turned.
        /// </summary>
        public float WheelDelta
        {
            get
            {
                return -movementVector.Z;
            }
        }

        /// <summary>
        /// Is the left button pressed down?
        /// </summary>
        public bool LeftButtonPressed
        {
            get
            {
                return leftButtonPressed;
            }
        }

        /// <summary>
        /// Is the right button pressed down?
        /// </summary>
        public bool RightButtonPressed
        {
            get
            {
                return rightButtonPressed;
            }
        }

        /// <summary>
        /// A number in the range [0,1]. The higher this number, the greater is the motion of the mouse
        /// smoothed.
        /// </summary>
        public float SmoothingFactor
        {
            get
            {
                return smoothingFactor;
            }
            set
            {
                smoothingFactor = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the mouse device.
        /// </summary>
        public MouseDevice( Form form )
        {
            mouseDevice = new DirectInput.Device( SystemGuid.Mouse );
            mouseDevice.SetCooperativeLevel( form, CooperativeLevelFlags.Background |
                CooperativeLevelFlags.NonExclusive );
            mouseDevice.SetDataFormat( DeviceDataFormat.Mouse );

            mouseDevice.Properties.AxisModeAbsolute = false;

            mouseDevice.Acquire();

            Update();
        }
        #endregion

        #region Update
        /// <summary>
        /// Polls the mouse device, and updates the statistics.
        /// </summary>
        public void Update()
        {
            MouseState state = mouseDevice.CurrentMouseState;

            // Update the wheel data
            movementVector.Z = state.Z;

            // Slowly interpolate from the current movement deltas to the new movement deltas.
            // This helps smooth the motion of anything controlled directly by the mouse.
            movementVector.X = movementVector.X * smoothingFactor + state.X * ( 1.0f - smoothingFactor );
            movementVector.Y = movementVector.Y * smoothingFactor + state.Y * ( 1.0f - smoothingFactor );

            // Check if the left or right buttons are pressed
            byte[] buttons = state.GetMouseButtons();
            leftButtonPressed = buttons.Length > 0 && buttons[ 0 ] != 0;
            rightButtonPressed = buttons.Length > 1 && buttons[ 1 ] != 0;
        }
        #endregion
    }
}