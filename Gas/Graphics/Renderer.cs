using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Gas.Helpers;

namespace Gas.Graphics
{
    #region Helpers
    /// <summary>
    /// Describes how projections from view space to screen space are to be performed.
    /// </summary>
    public enum ProjectionMode
    {
        /// <summary>
        /// Perspective projection. Used for true 3D graphics.
        /// </summary>
        Perspective,
        /// <summary>
        /// Orthogonal projection. Used for doing 2D graphics without resorting to
        /// transformed coordinates.
        /// </summary>
        Orthogonal
    }
    #endregion

    /// <summary>
    /// Initializes and manages Direct3D. Also provides comprehensive functionality 
    /// for rendering the scene. The renderer uses the programmable pipeline exclusively
    /// by way of the DirectX Effect framework and Gas's VisualEffect system.
    /// </summary>
    public class Renderer
    {
        #region Variables
        private D3DEnum d3dEnum = new D3DEnum();
        private DeviceSettings windowedSettings = null;
        private DeviceSettings fullscreenSettings = null;
        private DeviceSettings currentSettings = null;

        private Device device = null;
        private Control renderTarget = null;
        private DisplayMode displayMode;
        private bool windowed;

        private List<IGraphicsResource> graphicsObjects = new List<IGraphicsResource>();

        private bool canDoPS11 = false, canDoPS20 = false, canDoPS30 = false;
        private bool canDoVS11 = false, canDoVS20 = false, canDoVS30 = false;

        private float fieldOfView = ( float )Math.PI / 2.0f,
            nearPlane = 1.0f, farPlane = 100.0f;

        private Matrix worldMatrix = Matrix.Identity, viewMatrix = Matrix.Identity,
            projectionMatrix = Matrix.Identity;

        private ProjectionMode projectionMode = ProjectionMode.Perspective;

        private Effect currentEffect = null;

        private Microsoft.DirectX.Direct3D.Surface savedRenderTarget = null;

        private Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private Dictionary<string, VisualEffect> visualEffects = new Dictionary<string, VisualEffect>();

        private List<RenderPacket> renderPackets = new List<RenderPacket>();

        private List<Light> lights = new List<Light>();
        #endregion

        #region Events
        public event EventHandler WindowedChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the render device supports vertex shader version 1.0.
        /// </summary>
        public bool CanDoVS11
        {
            get
            {
                return canDoVS11;
            }
        }

        /// <summary>
        /// Gets whether the render device supports vertex shader version 2.0.
        /// </summary>
        public bool CanDoVS20
        {
            get
            {
                return canDoVS20;
            }
        }

        /// <summary>
        /// Gets whether the render device supports vertex shader version 3.0.
        /// </summary>
        public bool CanDoVS30
        {
            get
            {
                return canDoVS30;
            }
        }

        /// <summary>
        /// Gets whether the render device supports pixel shader version 1.1.
        /// </summary>
        public bool CanDoPS11
        {
            get
            {
                return canDoPS11;
            }
        }

        /// <summary>
        /// Gets whether the render device supports pixel shader version 2.0.
        /// </summary>
        public bool CanDoPS20
        {
            get
            {
                return canDoPS20;
            }
        }

        /// <summary>
        /// Gets whether the render device supports pixel shader version 3.0.
        /// </summary>
        public bool CanDoPS30
        {
            get
            {
                return canDoPS30;
            }
        }

        /// <summary>
        /// Gets and sets whether the Device is in windowed mode
        /// </summary>
        public bool Windowed
        {
            get
            {
                return windowed;
            }
            set
            {
                windowed = value;

                WindowedChanged.Invoke( this, null );

                if ( !windowed )
                {
                    // Going to fullscreen mode
                    ChangeDevice( fullscreenSettings );
                }
                else
                {
                    // Going to window mode
                    ChangeDevice( windowedSettings );
                }
            }
        }

        /// <summary>
        /// Gets the Direct3D device.
        /// </summary>
        public Device Device
        {
            get
            {
                return device;
            }
        }

        /// <summary>
        /// Gets the resolution for fullscreen mode.
        /// </summary>
        public Size FullscreenSize
        {
            get
            {
                return new Size( displayMode.Width, displayMode.Height );
            }
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public DeviceSettings CurrentSettings
        {
            get
            {
                return ( DeviceSettings )currentSettings.Clone();
            }
        }

        /// <summary>
        /// Gets and sets the windowed settings
        /// </summary>
        public DeviceSettings WindowedSettings
        {
            get
            {
                return ( DeviceSettings )windowedSettings.Clone();
            }
            set
            {
                windowedSettings = value;
            }
        }

        /// <summary>
        /// Gets and sets the fullscreen settings
        /// </summary>
        public DeviceSettings FullscreenSettings
        {
            get
            {
                return ( DeviceSettings )fullscreenSettings.Clone();
            }
            set
            {
                fullscreenSettings = value;
            }
        }

        /// <summary>
        /// Gets and sets the world matrix.
        /// </summary>
        public Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                if ( worldMatrix != value )
                {
                    worldMatrix = value;
                    device.Transform.World = worldMatrix;
                }
            }
        }

        /// <summary>
        /// Gets and sets the view matrix.
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                return viewMatrix;
            }
            set
            {
                viewMatrix = value;
                device.Transform.View = viewMatrix;
            }
        }

        /// <summary>
        /// Gets the inverse of the view matrix.
        /// </summary>
        public Matrix InverseViewMatrix
        {
            get
            {
                return Matrix.Invert( viewMatrix );
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get
            {
                return projectionMatrix;
            }
            set
            {
                projectionMatrix = value;
                device.Transform.Projection = projectionMatrix;
            }
        }

        /// <summary>
        /// Gets the world-view-projection matrix.
        /// </summary>
        public Matrix WorldViewProjectionMatrix
        {
            get
            {
                return worldMatrix * viewMatrix * projectionMatrix;
            }
        }

        /// <summary>
        /// Gets the field of view.
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return fieldOfView;
            }
        }

        /// <summary>
        /// Gets or sets the projection mode.
        /// </summary>
        public ProjectionMode ProjectionMode
        {
            get
            {
                return projectionMode;
            }
            set
            {
                projectionMode = value;
                BuildProjectionMatrix( FullscreenSize );
            }
        }

        /// <summary>
        /// Gets the current Effect.
        /// </summary>
        public Effect CurrentEffect
        {
            get
            {
                return currentEffect;
            }
        }

        /// <summary>
        /// Gets the list of lights.
        /// </summary>
        public List<Light> Lights
        {
            get
            {
                return lights;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the renderer by firing up Direct3D and loading all resources into memory, such as 
        /// materials and visual effects.
        /// </summary>
        public Renderer( bool windowed, Control renderTarget, int desiredWidth, int desiredHeight )
        {
            try
            {
                InitializeDirect3D( windowed, renderTarget, desiredWidth, desiredHeight );
            }
            catch ( Exception ex )
            {
                Log.Write( "Failed to initialize Direct3D. Exception text: " + ex.ToString() );
                throw ex;
            }

            try
            {
                LoadMaterials();
            }
            catch ( Exception ex )
            {
                Log.Write( "Failed to load Materials. Exception text: " + ex.ToString() );
                throw ex;
            }

            try
            {
                LoadVisualEffects();
            }
            catch ( Exception ex )
            {
                Log.Write( "Failed to load VisualEffects. Exception text: " + ex.ToString() );
                throw ex;
            }
        }
        #endregion

        #region Misc. graphics methods
        /// <summary>
        /// Sets up default view and projection matrices such that the camera is at (0,0,-5) facing the origin with
        /// (0,1,0) being the up vector.
        /// </summary>
        public void SetDefault2DParameters()
        {
            ProjectionMode = ProjectionMode.Orthogonal;
            ViewMatrix = Matrix.LookAtLH( new Vector3( 0, 0, -5.0f ), new Vector3(),
                new Vector3( 0, 1, 0 ) );
        }

        /// <summary>
        /// Adds an IGraphicsObject to the internal list of graphics objects.
        /// </summary>
        public void AddGraphicsObject( IGraphicsResource graphicsObject )
        {
            graphicsObjects.Add( graphicsObject );
        }

        /// <summary>
        /// Saves a copy of the render target so that it can be restored later.
        /// </summary>
        public void SaveRenderTarget()
        {
            savedRenderTarget = device.GetRenderTarget( 0 );
        }

        /// <summary>
        /// Restores the render target to one saved with SaveRenderTarget().
        /// </summary>
        public void RestoreRenderTarget()
        {
            if ( savedRenderTarget != null )
                device.SetRenderTarget( 0, savedRenderTarget );
            else
                Log.Write( "Attempted to restore a null render target." );
        }

        /// <summary>
        /// Sets the screen (i.e., back-buffer) as the render target.
        /// </summary>
        public void SetScreenAsRenderTarget()
        {
            device.SetRenderTarget( 0, device.GetBackBuffer( 0, 0, BackBufferType.Mono ) );
        }

        /// <summary>
        /// Builds the projection matrix according to the projection mode.
        /// </summary>
        private void BuildProjectionMatrix( Size renderTargetSize )
        {
            switch ( projectionMode )
            {
                case ProjectionMode.Perspective:
                    float aspectRatio = 1.0f;

                    if ( renderTargetSize.Height != 0 )
                        aspectRatio = ( float )renderTargetSize.Width / ( float )renderTargetSize.Height;

                    ProjectionMatrix = Matrix.PerspectiveFovLH( fieldOfView, aspectRatio, nearPlane, farPlane );

                    break;

                case ProjectionMode.Orthogonal:
                    ProjectionMatrix = Matrix.OrthoLH( renderTargetSize.Width,
                        renderTargetSize.Height, nearPlane, farPlane );
                    break;
            }
        }

        /// <summary>
        /// Clears optionally the target, depth buffer and/or stencil.
        /// </summary>
        public void Clear( ClearFlags clearFlags, Color targetColor,
            float zClear, int stencilClear )
        {
            device.Clear( clearFlags, targetColor, zClear, stencilClear );
        }

        /// <summary>
        /// Clears the target to black and the z-buffer to 1.0f.
        /// </summary>
        public void Clear()
        {
            Clear( ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0 );
        }

        /// <summary>
        /// Binds a texture to an Effect parameter.
        /// </summary>
        public void BindTexture( string effectParameterName, Texture texture )
        {
            if ( currentEffect != null )
                currentEffect.SetValue( effectParameterName, texture );
        }

        /// <summary>
        /// Begins rendering with an effect. If 'effect' is null, then the fixed-function pipeline is used.
        /// </summary>
        public void Begin( Effect effect )
        {
            //device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            if ( effect != null )
            {
                currentEffect = effect;
                currentEffect.BeginTechnique();
            }
        }

        /// <summary>
        /// Sets the Effect pass to render. Must be called between Begin() and End() calls.
        /// </summary>
        public void SetPass( int passNumber )
        {
            if ( currentEffect != null )
                currentEffect.Pass( passNumber );
            else
                Log.Write( "Attempted to render a pass with a null Effect." );
        }

        /// <summary>
        /// Ends rendering with an effect.
        /// </summary>
        public void End()
        {
            if ( currentEffect != null )
                currentEffect.EndTechnique();

            device.EndScene();
        }

        /// <summary>
        /// Presents the scene to the front buffer.
        /// </summary>
        public void Present()
        {
            device.Present();
        }

        /// <summary>
        /// Loads all the materials stored in the Materials directory. This assumes that all files in said
        /// directory are material files.
        /// </summary>
        private void LoadMaterials()
        {
            foreach ( string filename in Directory.GetFiles( "Materials" ) )
            {
                string matName = StringHelper.GetNameOfFile( filename );
                materials.Add( matName, GlobalResourceCache.CreateMaterialFromFile( this, filename ) );
            }
        }

        /// <summary>
        /// Loads all the VisualEffect DLL files stored in the VisualEffects directory. This assumes that all
        /// files in said directory are DLL files containing an implementation of the VisualEffect interface.
        /// </summary>
        private void LoadVisualEffects()
        {
            foreach ( string filename in Directory.GetFiles( "VisualEffects" ) )
            {
                string visualEffectName = StringHelper.GetNameOfFile( filename );
                Type visualEffect = null;
                Assembly visualEffectAssembly = Assembly.LoadFile( Application.StartupPath + @"\" + filename );

                foreach ( Type type in visualEffectAssembly.GetTypes() )
                {
                    object[] attrs = type.GetCustomAttributes( typeof( MainVisualEffectClassAttribute ), false );
                    foreach ( object objAttr in attrs )
                    {
                        Attribute attr = ( Attribute )objAttr;
                        if ( ( attr as MainVisualEffectClassAttribute ) != null )
                            visualEffect = type;
                    }
                }

                visualEffects.Add( visualEffectName,
                    ( VisualEffect )( Activator.CreateInstance( visualEffect, new object[] { this } ) ) );
            }
        }

        /// <summary>
        /// Adds a render packet to the internal list to be rendered upon calling Renderer.Render().
        /// </summary>
        public void AddRenderPacket( RenderPacket renderPacket )
        {
            renderPackets.Add( renderPacket );
        }

        /// <summary>
        /// Adds a material to the internal list of materials.
        /// </summary>
        /// <param name="material"></param>
        public void AddMaterial( Material material, string name )
        {
            materials.Add( name, material );
        }

        /// <summary>
        /// Renders all the RenderPackets. First, the packets are sorted according to material type to
        /// minimize switching shaders at runtime. Second, Render() is called on the IRenderable instances
        /// in all of the packets after setting the local world transforms.
        /// 
        /// Once finished, the render packet list is emptied for next frame.
        /// </summary>
        public void Render()
        {
            #region Sort render packets into lists according to material type
            Dictionary<string, List<RenderPacket>> lists = new Dictionary<string, List<RenderPacket>>();

            foreach ( RenderPacket renderPacket in renderPackets )
            {
                if ( !lists.ContainsKey( renderPacket.MaterialName ) )
                    lists.Add( renderPacket.MaterialName, new List<RenderPacket>() );

                lists[ renderPacket.MaterialName ].Add( renderPacket );
            }
            #endregion

            #region Render all the render packets
            foreach ( string materialName in lists.Keys )
            {
                VisualEffect visualEffect = visualEffects[ materials[ materialName ].VisualEffectName ];
                visualEffect.BeginRenderScene();

                foreach ( RenderPacket renderPacket in lists[ materialName ] )
                {
                    WorldMatrix = renderPacket.LocalTransform;

                    if ( renderPacket.ExtraData.Count == 0 )
                        visualEffect.BeginRenderObject( materials[ materialName ] );
                    else
                        visualEffect.BeginRenderObject( materials[ materialName ], renderPacket.ExtraData );

                    renderPacket.RenderObject.Render();
                    visualEffect.EndRenderObject();
                }

                visualEffect.EndRenderScene();
            }
            #endregion

            renderPackets.Clear();
        }

        /// <summary>
        /// Renders all the RenderPackets.
        /// </summary>
        /// <param name="sortMaterials">Should the packets be sorted according to material type to minimize
        /// switching shaders at runtime?</param>
        public void Render( bool sortMaterials )
        {
            if ( sortMaterials )
            {
                Render();
                return;
            }

            foreach ( RenderPacket renderPacket in renderPackets )
            {
                VisualEffect visualEffect = visualEffects[ materials[ renderPacket.MaterialName ].VisualEffectName ];
                visualEffect.BeginRenderScene();

                WorldMatrix = renderPacket.LocalTransform;

                if ( renderPacket.ExtraData.Count == 0 )
                    visualEffect.BeginRenderObject( materials[ renderPacket.MaterialName ] );
                else
                    visualEffect.BeginRenderObject( materials[ renderPacket.MaterialName ], renderPacket.ExtraData );

                renderPacket.RenderObject.Render();
                visualEffect.EndRenderObject();

                visualEffect.EndRenderScene();
            }

            renderPackets.Clear();
        }

        /// <summary>
        /// Adds a new light to the light list and returns a reference to it.
        /// </summary>
        public Light RegisterNewLight( float range, float intensity )
        {
            Light light = new Light( this, range, intensity );
            lights.Add( light );
            return light;
        }

        /// <summary>
        /// Adds a new light to the light list and returns a reference to it.
        /// </summary>
        public Light RegisterNewLight( float range, float intensity, Vector2 position, Color color )
        {
            Light light = new Light( this, range, intensity, position, color );
            lights.Add( light );
            return light;
        }

        /// <summary>
        /// Removes a light from the internal list.
        /// </summary>
        public void RemoveLight( Light light )
        {
            lights.Remove( light );
        }

        /// <summary>
        /// Renders a line going from start to end with the specified color.
        /// </summary>
        public void RenderLine( Vector3 start, Vector3 end, Color color )
        {
            CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[ 2 ];
            vertices[ 0 ].Position = start;
            vertices[ 0 ].Color = color.ToArgb();
            vertices[ 1 ].Position = end;
            vertices[ 1 ].Color = color.ToArgb();

            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.DrawUserPrimitives( PrimitiveType.LineList, 1, vertices );
        }

        public void RenderLine( Vector2 start, Vector2 end, Color color )
        {
            RenderLine( new Vector3( start.X, start.Y, 0.0f ), new Vector3( end.X, end.Y, 0.0f ), color );
        }
        #endregion

        #region Graphics Object Creation Methods
        // These methods just make creating certain objects easier by removing the need to repeatedly
        // pass a Renderer reference to constructors.

        /// <summary>
        /// Creates a new Gas.Graphics.Effect.
        /// </summary>
        public Effect CreateEffect( string filename )
        {
            return new Effect( this, filename );
        }

        /// <summary>
        /// Constructs a font object using the desired font family. If the desired family
        /// is not supported, the fallback family defined in the Settings file is used.
        /// </summary>
        public Gas.Graphics.Font CreateFont( string familyName, int height )
        {
            return new Font( this, familyName, height );
        }

        /// <summary>
        /// Creates a new material using the default parameters.
        /// </summary>
        public Material CreateMaterial()
        {
            return new Material( this );
        }

        /// <summary>
        /// Builds a material using a standard Direct3D material structure and the name of the VisualEffect.
        /// </summary>
        public Material CreateMaterial( Microsoft.DirectX.Direct3D.Material material, string visualEffectName )
        {
            return new Material( this, material, visualEffectName );
        }

        /// <summary>
        /// Creates a new Mesh and allocates all the memory it will need.
        /// </summary>
        public Mesh CreateMesh( int numVertices, int numTriangles )
        {
            return new Mesh( this, numVertices, numTriangles );
        }

        /// <summary>
        /// Builds a rectangular mesh, centered around the origin.
        /// </summary>
        public Mesh CreateRectangularMesh( Color color, float width, float height, float textureMapTiling )
        {
            return Mesh.Rectangle( this, color, width, height, textureMapTiling );
        }

        /// <summary>
        /// Builds a rectangular mesh, centered around the origin.
        /// </summary>
        public Mesh CreateRectangularMesh( Color color, float width, float height )
        {
            return Mesh.Rectangle( this, color, width, height );
        }

        /// <summary>
        /// Builds a circular mesh centered around the origin.
        /// </summary>
        public Mesh CreateCircularMesh( Color color, float radius, int numSubdivisions, float textureMapTiling )
        {
            return Mesh.Circle( this, color, radius, numSubdivisions, textureMapTiling );
        }

        /// <summary>
        /// Builds a circular mesh centered around the origin.
        /// </summary>
        public Mesh CreateCircularMesh( Color color, float radius, int numSubdivisions )
        {
            return Mesh.Circle( this, color, radius, numSubdivisions );
        }

        /// <summary>
        /// Creates a new surface.
        /// </summary>
        public Surface CreateSurface( string materialName, Size size )
        {
            return new Surface( this, materialName, size );
        }

        /// <summary>
        /// Creates a new surface.
        /// </summary>
        public Surface CreateSurface( string materialName, Size size, float textureMapTiling )
        {
            return new Surface( this, materialName, size, textureMapTiling );
        }

        /// <summary>
        /// Creates a texture from a file.
        /// </summary>
        /// <param name="filename">The name of the texture file to load.</param>
        public Texture CreateTextureFromFile( string filename )
        {
            return new Texture( this, filename );
        }

        /// <summary>
        /// Creates a texture as a render target.
        /// </summary>
        /// <param name="width">The width, in pixels, of the texture.</param>
        /// <param name="height">The height, in pixels, of the texture.</param>
        /// <param name="alpha">Should memory be allocated for alpha color components?</param>
        public Texture CreateTextureAsRenderTarget( int width, int height, bool alpha )
        {
            return new Texture( this, width, height, alpha );
        }
        #endregion

        #region Direct3D Initialization
        /// <summary>
        /// Initializes Direct3D using passed in settings.
        /// </summary>
        private void InitializeDirect3D( bool windowed, Control renderTarget, int desiredWidth, int desiredHeight )
        {
            this.windowed = windowed;
            this.renderTarget = renderTarget;
            this.displayMode = Manager.Adapters[ 0 ].CurrentDisplayMode;

            //if ( !windowed )
            //{
                displayMode.Width = desiredWidth;
                displayMode.Height = desiredHeight;
            //}

            d3dEnum.EnumerateAdapters();

            // Create the device settings
            windowedSettings = FindBestWindowedSettings();
            fullscreenSettings = FindBestFullscreenSettings();
            currentSettings = windowed ? windowedSettings : fullscreenSettings;

            try
            {
                device = new Device( ( int )currentSettings.AdapterOrdinal, currentSettings.DeviceType,
                    renderTarget, currentSettings.BehaviorFlags, currentSettings.PresentParameters );
            }
            catch ( DirectXException )
            {
                throw new DirectXException( "Unable to create the Direct3D device." );
            }

            // Cancel automatic device reset on resize
            device.DeviceResizing += new System.ComponentModel.CancelEventHandler( CancelResize );

            device.DeviceLost += new EventHandler( OnDeviceLost );
            device.DeviceReset += new EventHandler( OnDeviceReset );
            device.Disposing += new EventHandler( OnDeviceDisposing );

            // What vertex and pixel shader versions are supported?
            Caps caps = Manager.GetDeviceCaps( ( int )currentSettings.AdapterOrdinal, currentSettings.DeviceType );

            canDoPS11 = caps.PixelShaderVersion >= new Version( 1, 1 );
            canDoPS20 = caps.PixelShaderVersion >= new Version( 2, 0 );
            canDoPS30 = caps.PixelShaderVersion >= new Version( 3, 0 );

            canDoVS11 = caps.VertexShaderVersion >= new Version( 1, 1 );
            canDoVS20 = caps.VertexShaderVersion >= new Version( 2, 0 );
            canDoVS30 = caps.VertexShaderVersion >= new Version( 3, 0 );

            BuildProjectionMatrix( new Size( desiredWidth, desiredHeight ) );
        }

        /// <summary>
        /// Disposes of all IGraphicsObjects.
        /// </summary>
        void OnDeviceDisposing( object sender, EventArgs e )
        {
            foreach ( IGraphicsResource graphicsObject in graphicsObjects )
                graphicsObject.Dispose();
        }

        /// <summary>
        /// Handles a device reset. Informs all IGraphicsObjects of the event.
        /// </summary>
        void OnDeviceReset( object sender, EventArgs e )
        {
            BuildProjectionMatrix( FullscreenSize );

            foreach ( IGraphicsResource graphicsObject in graphicsObjects )
                graphicsObject.OnDeviceReset();
        }

        /// <summary>
        /// Handles a lost device. Informs all IGraphicsObjects of the event.
        /// </summary>
        void OnDeviceLost( object sender, EventArgs e )
        {
            foreach ( IGraphicsResource graphicsObject in graphicsObjects )
                graphicsObject.OnDeviceLost();
        }

        /// <summary>
        /// Changes the device with the new settings.
        /// </summary>
        public void ChangeDevice( DeviceSettings newSettings )
        {
            windowed = newSettings.PresentParameters.Windowed;

            if ( newSettings.PresentParameters.Windowed )
                windowedSettings = ( DeviceSettings )newSettings.Clone();
            else
                fullscreenSettings = ( DeviceSettings )newSettings.Clone();

            if ( device != null )
            {
                device.Dispose();
                device = null;
            }

            try
            {
                device = new Device( ( int )newSettings.AdapterOrdinal, newSettings.DeviceType, renderTarget,
                    newSettings.BehaviorFlags, newSettings.PresentParameters );

                // Cancel automatic device reset on resize
                device.DeviceResizing += new System.ComponentModel.CancelEventHandler( CancelResize );

                device.DeviceLost += new EventHandler( OnDeviceLost );
                device.DeviceReset += new EventHandler( OnDeviceReset );
                device.Disposing += new EventHandler( OnDeviceDisposing );

                OnDeviceReset( this, null );
            }
            catch ( DirectXException )
            {
                throw new DirectXException( "Unable to recreate the Direct3D device while changing settings" );
            }

            currentSettings = windowed ? windowedSettings : fullscreenSettings;
        }

        /// <summary>
        /// Rebuilds present parameters in preparation for a device reset.
        /// </summary>
        public void ResetPresentParameters()
        {
            if ( windowed )
            {
                currentSettings.PresentParameters.BackBufferWidth = 0;
                currentSettings.PresentParameters.BackBufferHeight = 0;
            }
        }

        /// <summary>
        /// Resets the device.
        /// </summary>
        public void Reset()
        {
            if ( device != null )
            {
                ResetPresentParameters();
                device.Reset( currentSettings.PresentParameters );
            }
        }

        /// <summary>
        /// Finds the best windowed Device settings supported by the system.
        /// </summary>
        /// <returns>
        /// A DeviceSettings class full with the best supported windowed settings.
        /// </returns>
        private DeviceSettings FindBestWindowedSettings()
        {
            DeviceSettingsEnum bestSettings = null;
            bool foundBest = false;
            // Loop through each adapter
            foreach ( AdapterEnum a in d3dEnum.Adapters )
            {
                // Loop through each device
                foreach ( DeviceEnum d in a.DeviceEnumList )
                {
                    // Loop through each device settings configuration
                    foreach ( DeviceSettingsEnum s in d.SettingsList )
                    {
                        // Must be windowed mode and the AdapterFormat must match current DisplayMode Format
                        if ( !s.Windowed || ( s.AdapterFormat != displayMode.Format ) )
                        {
                            continue;
                        }

                        // The best DeviceSettingsEnum is a DeviceType.Hardware Device
                        // where its BackBufferFormat is the same as the AdapterFormat
                        if ( ( bestSettings == null ) ||
                             ( ( s.DeviceType == DeviceType.Hardware ) && ( s.AdapterFormat == s.BackBufferFormat ) ) ||
                             ( ( bestSettings.DeviceType != DeviceType.Hardware ) &&
                             ( s.DeviceType == DeviceType.Hardware ) ) )
                        {
                            if ( !foundBest )
                            {
                                bestSettings = s;
                            }

                            if ( ( s.DeviceType == DeviceType.Hardware ) && ( s.AdapterFormat == s.BackBufferFormat ) )
                            {
                                foundBest = true;
                            }
                        }
                    }
                }
            }

            if ( bestSettings == null )
            {
                throw new DirectXException( "Unable to find any supported window mode settings." );
            }

            // Store the best settings
            DeviceSettings windowedSettings = new DeviceSettings();

            windowedSettings.AdapterFormat = bestSettings.AdapterFormat;
            windowedSettings.AdapterOrdinal = bestSettings.AdapterOrdinal;
            windowedSettings.BehaviorFlags = ( CreateFlags )bestSettings.VertexProcessingTypeList[ 0 ];
            windowedSettings.Caps = bestSettings.DeviceInformation.Caps;
            windowedSettings.DeviceType = bestSettings.DeviceType;

            windowedSettings.PresentParameters = new PresentParameters();

            windowedSettings.PresentParameters.AutoDepthStencilFormat =
                ( DepthFormat )bestSettings.DepthStencilFormatList[ 0 ];
            windowedSettings.PresentParameters.BackBufferCount = 1;
            windowedSettings.PresentParameters.BackBufferFormat = bestSettings.AdapterFormat;
            windowedSettings.PresentParameters.BackBufferHeight = 0;
            windowedSettings.PresentParameters.BackBufferWidth = 0;
            windowedSettings.PresentParameters.DeviceWindow = renderTarget;
            windowedSettings.PresentParameters.EnableAutoDepthStencil = true;
            windowedSettings.PresentParameters.FullScreenRefreshRateInHz = 0;
            windowedSettings.PresentParameters.MultiSample = ( MultiSampleType )bestSettings.MultiSampleTypeList[ 0 ];
            windowedSettings.PresentParameters.MultiSampleQuality = 0;
            windowedSettings.PresentParameters.PresentationInterval =
                ( PresentInterval )bestSettings.PresentIntervalList[ 0 ];
            windowedSettings.PresentParameters.PresentFlag = PresentFlag.DiscardDepthStencil;
            windowedSettings.PresentParameters.SwapEffect = SwapEffect.Discard;
            windowedSettings.PresentParameters.Windowed = true;

            return windowedSettings;
        }

        /// <summary>
        /// Finds the best fullscreen Device settings supported by the system.
        /// </summary>
        /// <returns>
        /// A DeviceSettings class full with the best supported fullscreen settings.
        /// </returns>
        private DeviceSettings FindBestFullscreenSettings()
        {
            DeviceSettingsEnum bestSettings = null;
            bool foundBest = false;

            // Loop through each adapter
            foreach ( AdapterEnum a in d3dEnum.Adapters )
            {
                // Loop through each device
                foreach ( DeviceEnum d in a.DeviceEnumList )
                {
                    // Loop through each device settings configuration
                    foreach ( DeviceSettingsEnum s in d.SettingsList )
                    {
                        // Must be fullscreen mode
                        if ( s.Windowed )
                        {
                            continue;
                        }

                        // To make things easier, we'll say the best DeviceSettingsEnum 
                        // is a DeviceType.Hardware Device whose AdapterFormat is the same as the
                        // current DisplayMode Format and whose BackBufferFormat matches the
                        // AdapterFormat
                        if ( ( bestSettings == null ) ||
                             ( ( s.DeviceType == DeviceType.Hardware ) && ( s.AdapterFormat == displayMode.Format ) ) ||
                             ( ( bestSettings.DeviceType != DeviceType.Hardware ) &&
                             ( s.DeviceType == DeviceType.Hardware ) ) )
                        {
                            if ( !foundBest )
                            {
                                bestSettings = s;
                            }

                            if ( ( s.DeviceType == DeviceType.Hardware ) &&
                                ( s.AdapterFormat == displayMode.Format ) &&
                                ( s.BackBufferFormat == s.AdapterFormat ) )
                            {
                                foundBest = true;
                            }
                        }
                    }
                }
            }
            if ( bestSettings == null )
            {
                throw new DirectXException( "Unable to find any supported fullscreen mode settings." );
            }

            // Store the best settings
            DeviceSettings fullscreenSettings = new DeviceSettings();
            fullscreenSettings.AdapterFormat = bestSettings.AdapterFormat;
            fullscreenSettings.AdapterOrdinal = bestSettings.AdapterOrdinal;
            fullscreenSettings.BehaviorFlags = ( CreateFlags )bestSettings.VertexProcessingTypeList[ 0 ];
            fullscreenSettings.Caps = bestSettings.DeviceInformation.Caps;
            fullscreenSettings.DeviceType = bestSettings.DeviceType;

            fullscreenSettings.PresentParameters = new PresentParameters();
            fullscreenSettings.PresentParameters.AutoDepthStencilFormat =
                ( DepthFormat )bestSettings.DepthStencilFormatList[ 0 ];
            fullscreenSettings.PresentParameters.BackBufferCount = 1;
            fullscreenSettings.PresentParameters.BackBufferFormat = bestSettings.AdapterFormat;
            fullscreenSettings.PresentParameters.BackBufferHeight = displayMode.Height;
            fullscreenSettings.PresentParameters.BackBufferWidth = displayMode.Width;
            fullscreenSettings.PresentParameters.DeviceWindow = renderTarget;
            fullscreenSettings.PresentParameters.EnableAutoDepthStencil = true;
            fullscreenSettings.PresentParameters.FullScreenRefreshRateInHz = displayMode.RefreshRate;
            fullscreenSettings.PresentParameters.MultiSample =
                ( MultiSampleType )bestSettings.MultiSampleTypeList[ 0 ];
            fullscreenSettings.PresentParameters.MultiSampleQuality = 0;
            fullscreenSettings.PresentParameters.PresentationInterval =
                ( PresentInterval )bestSettings.PresentIntervalList[ 0 ];
            fullscreenSettings.PresentParameters.PresentFlag = PresentFlag.DiscardDepthStencil;
            fullscreenSettings.PresentParameters.SwapEffect = SwapEffect.Discard;
            fullscreenSettings.PresentParameters.Windowed = false;

            return fullscreenSettings;
        }

        /// <summary>
        /// Cancels the automatic device reset on resize
        /// </summary>
        private void CancelResize( object sender, System.ComponentModel.CancelEventArgs e )
        {
            e.Cancel = true;
        }
        #endregion
    }
}