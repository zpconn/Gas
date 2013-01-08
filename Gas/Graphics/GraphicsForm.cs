using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Gas.Helpers;
using Gas.Input;

namespace Gas.Graphics
{
    /// <summary>
    /// A form that is ready-to-go for graphics rendering.
    /// </summary>
    public class GraphicsForm : Form
    {
        #region Variables
        protected Renderer renderer = null;
        protected HighResTimer timer = new HighResTimer();
        protected bool running = true;
        protected bool initialized = false;
        #endregion

        #region Methods for handling Direct3D
        /// <summary>
        /// Prepares the form so that it behaves better with Direct3D; initializes
        /// the renderer; and finally hooks up appropriate event handlers for handling
        /// the Direct3D device.
        /// </summary>
        private void SetupForDirect3D( bool windowed, int desiredWidth, int desiredHeight,
            string windowTitle )
        {
            ClientSize = new Size( desiredWidth, desiredHeight );
            this.Text = windowTitle;

            try
            {
                renderer = new Renderer( windowed, this, desiredWidth, desiredHeight );
            }
            catch
            {
                Log.Write( "Unable to initialize the renderer." );
                throw new DirectXException( "Unable to initialize the renderer." );
            }

            this.Resize += new EventHandler( OnResize );
            renderer.Device.DeviceResizing += new System.ComponentModel.CancelEventHandler( OnDeviceResizing );
            renderer.WindowedChanged += new EventHandler( OnWindowedChanged );

            OnWindowedChanged( this, null );
        }

        void OnWindowedChanged( object sender, EventArgs e )
        {
            if ( !renderer.Windowed )
            {
                this.FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
            }
        }

        /// <summary>
        /// Invoked when the device detects the render target being resized. This handler
        /// checks if the device has been lost, and if so, cancels the event. It also
        /// imposes a minimum form size of 320x200.
        /// </summary>
        void OnDeviceResizing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if ( !initialized )
                return;

            try
            {
                if ( !ReadyToRender )
                    e.Cancel = true;

                if ( this.WindowState != FormWindowState.Minimized )
                {
                    // Impose a minimum size of 320x200

                    if ( this.Size.Width < 320 )
                        this.Size = new Size( 320, this.Size.Height );

                    if ( this.Size.Height < 200 )
                        this.Size = new Size( this.Size.Width, 200 );
                }
            }
            catch ( Exception ex )
            {
                Log.Write( "OnDeviceResizing() failed: " + ex.ToString() );
            }
        }

        /// <summary>
        /// Invoked when the form is resized. This handler updates the environment,
        /// and renders the scene if safe.
        /// </summary>
        void OnResize( object sender, EventArgs e )
        {
            if ( !initialized )
                return;

            timer.Update();
            UpdateEnvironment();

            // Only reset the device and render the scene if it is safe to do so.
            if ( ReadyToRender )
            {
                renderer.Reset();
                Render3DEnvironment();
            }
        }
        #endregion

        #region Render loop
        /// <summary>
        /// The game loop. Hooks up the appropriate event handlers, and runs the application.
        /// </summary>
        public void Run( bool windowed, int desiredWidth, int desiredHeight,
            string windowTitle )
        {
            BigScreenMessage bigMessage = new BigScreenMessage( "Loading Resources. Please Wait." );
            bigMessage.Show();
            bigMessage.Update();

            SetupForDirect3D( windowed, desiredWidth, desiredHeight, windowTitle );
            InitializeGame();

            initialized = true;

            bigMessage.Close();

            this.Closed += new EventHandler( OnClosed );
            Application.Idle += new EventHandler( OnApplicationIdle );
            Application.Run( this );
        }

        void OnClosed( object sender, EventArgs e )
        {
            running = false;
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// Updates and renders the 3D environment on the Application.Idle event.
        /// </summary>
        private void OnApplicationIdle( object sender, EventArgs e )
        {
            if ( !running )
            {
                this.Close();
            }

            while ( AppStillIdle && ReadyToRender )
            {
                // Render a frame during idle time (no messages are waiting)

                timer.Update();
                UpdateEnvironment();

                Render3DEnvironment();
            }
        }

        /// <summary>
        /// Is the application idle, or are there messages waiting to be handled?
        /// </summary>
        private bool AppStillIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage( out msg, IntPtr.Zero, 0, 0, 0 );
            }
        }

        /// <summary>
        /// Are we ready to render this frame, or should we hold back until either
        /// the window and/or the Direct3D device are better behaved?
        /// </summary>
        private bool ReadyToRender
        {
            get
            {
                int result;
                renderer.Device.CheckCooperativeLevel( out result );

                return !( this.WindowState == FormWindowState.Minimized ||
                    result == ( int )ResultCode.DeviceLost || !initialized ||
                    !running );
            }
        }
        #endregion

        #region Virtual methods for running the game
        /// <summary>
        /// Sets up the game. Called after Direct3D initialization.
        /// </summary>
        protected virtual void InitializeGame() { }

        /// <summary>
        /// Updates the game environment.
        /// </summary>
        protected virtual void UpdateEnvironment() { }

        /// <summary>
        /// Renders the game environment.
        /// </summary>
        protected virtual void Render3DEnvironment() { }
        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GraphicsForm
            // 
            this.ClientSize = new System.Drawing.Size( 292, 266 );
            this.MinimizeBox = false;
            this.Name = "GraphicsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout( false );
        }
    }
}