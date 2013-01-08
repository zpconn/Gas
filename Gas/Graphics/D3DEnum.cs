using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Gas.Graphics
{
    #region Helper classes
    /// <summary>
    /// Represents a system adapter.
    /// </summary>
    public class AdapterEnum
    {
        public uint AdapterOrdinal;
        public AdapterDetails AdapterInformation;
        public List<DisplayMode> DisplayModeList = new List<DisplayMode>();
        public List<DeviceEnum> DeviceEnumList = new List<DeviceEnum>();
        public string Description;
    }

    /// <summary>
    /// Represents the capabilities and description of a single device.
    /// </summary>
    public class DeviceEnum
    {
        public uint AdapterOrdinal;
        public DeviceType DeviceType;
        public Caps Caps;
        public List<DeviceSettingsEnum> SettingsList = new List<DeviceSettingsEnum>();
    }

    /// <summary>
    /// Represents the various settings and configurations a device can have.
    /// </summary>
    public class DeviceSettingsEnum
    {
        public uint AdapterOrdinal;
        public DeviceType DeviceType;
        public Format AdapterFormat;
        public Format BackBufferFormat;
        public bool Windowed;

        public List<DepthFormat> DepthStencilFormatList = new List<DepthFormat>();
        public List<MultiSampleType> MultiSampleTypeList = new List<MultiSampleType>();
        public List<int> MultiSampleQualityList = new List<int>();
        public List<PresentInterval> PresentIntervalList = new List<PresentInterval>();
        public List<CreateFlags> VertexProcessingTypeList = new List<CreateFlags>();

        public AdapterEnum AdapterInformation = null;
        public DeviceEnum DeviceInformation = null;
    }

    /// <summary>
    /// Represents the settings for a single device configuration.
    /// </summary>
    public class DeviceSettings : ICloneable
    {
        public uint AdapterOrdinal;
        public Format AdapterFormat;
        public CreateFlags BehaviorFlags;
        public Caps Caps;
        public DeviceType DeviceType;
        public PresentParameters PresentParameters;

        /// <summary>
        /// Clones this instance of DeviceSettings.
        /// </summary>
        /// <returns>The clone.</returns>
        public object Clone()
        {
            DeviceSettings clone = new DeviceSettings();

            clone.AdapterOrdinal = AdapterOrdinal;
            clone.AdapterFormat = AdapterFormat;
            clone.BehaviorFlags = BehaviorFlags;
            clone.Caps = Caps;
            clone.DeviceType = DeviceType;
            clone.PresentParameters = PresentParameters;

            return clone;
        }
    }

    /// <summary>
    /// Sorts display modes.
    /// </summary>
    public class DisplayModeSorter : IComparer<DisplayMode>
    {
        /// <summary>
        /// Compares two display modes.
        /// </summary>
        public int Compare( DisplayMode d1, DisplayMode d2 )
        {
            if ( d1.Width > d2.Width )
                return 1;
            if ( d1.Width < d2.Width )
                return -1;

            if ( d1.Height > d2.Height )
                return 1;
            if ( d1.Height < d2.Height )
                return -1;

            if ( d1.Format > d2.Format )
                return 1;
            if ( d1.Format < d2.Format )
                return -1;

            if ( d1.RefreshRate > d2.RefreshRate )
                return 1;
            if ( d1.RefreshRate < d2.RefreshRate )
                return -1;

            return 0;
        }
    }
    #endregion

    /// <summary>
    /// Enumerates adapter and device capabilities.
    /// </summary>
    public class D3DEnum
    {
        #region Variables
        private List<AdapterEnum> adapters = new List<AdapterEnum>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the list of supported adapters.
        /// </summary>
        public List<AdapterEnum> Adapters
        {
            get
            {
                return adapters;
            }
        }
        #endregion

        #region Enumeration Arrays
        public static readonly Format[] AdapterFormats = new Format[] 
        {
            Format.X8R8G8B8, Format.X1R5G5B5, Format.R5G6B5, Format.A2R10G10B10  
        };

        public static readonly Format[] BackBufferFormats = new Format[] 
        {
            Format.A8R8G8B8, Format.X8R8G8B8, Format.R8G8B8, Format.A2R10G10B10, 
            Format.A8R3G3B2, Format.A4R4G4B4, Format.A1R5G5B5, Format.X4R4G4B4, 
            Format.X1R5G5B5, Format.R5G6B5, Format.R3G3B2
        };

        public static readonly DeviceType[] DeviceTypes = new DeviceType[] 
        {
            DeviceType.Hardware, DeviceType.Software, DeviceType.Reference
        };

        public static readonly MultiSampleType[] MultiSampleTypes = new MultiSampleType[]
        {
            MultiSampleType.None, MultiSampleType.NonMaskable, MultiSampleType.TwoSamples, 
            MultiSampleType.ThreeSamples, MultiSampleType.FourSamples, MultiSampleType.FiveSamples, 
            MultiSampleType.SixSamples, MultiSampleType.SevenSamples, MultiSampleType.EightSamples, 
            MultiSampleType.NineSamples, MultiSampleType.TenSamples, MultiSampleType.ElevenSamples, 
            MultiSampleType.ThirteenSamples, MultiSampleType.TwelveSamples, MultiSampleType.FourteenSamples, 
            MultiSampleType.FifteenSamples, MultiSampleType.SixteenSamples
        };

        public static readonly DepthFormat[] DepthFormats = new DepthFormat[]
        {
            DepthFormat.D32, DepthFormat.D24X4S4, DepthFormat.D24X8, DepthFormat.D24S8, 
            DepthFormat.D16, DepthFormat.D15S1
        };

        public static readonly PresentInterval[] PresentIntervals = new PresentInterval[]
        {
            PresentInterval.Immediate, PresentInterval.Default, PresentInterval.One,
            PresentInterval.Two, PresentInterval.Three, PresentInterval.Four 
        };
        #endregion

        #region Enumeration methods
        /// <summary>
        /// Enumerates all the adapters in the system.
        /// </summary>
        public void EnumerateAdapters()
        {
            foreach ( AdapterInformation adapterInfo in Manager.Adapters )
            {
                AdapterEnum currentAdapter = new AdapterEnum();

                // Store Adapter Ordinal and Information
                currentAdapter.AdapterOrdinal = ( uint )adapterInfo.Adapter;
                currentAdapter.AdapterInformation = adapterInfo.Information;
                currentAdapter.Description = adapterInfo.Information.Description;

                // Get all the DisplayModes the adapter supports
                ArrayList adapterFormatList = EnumerateDisplayModes( adapterInfo, currentAdapter );
                // Get all the devices the adapter can make
                EnumerateDevices( currentAdapter, adapterFormatList );

                adapters.Add( currentAdapter );
            }
        }

        /// <summary>
        /// Enumerates all the supported display modes for a given adapter.
        /// </summary>
        /// <param name="a">The adapter to enumerate.</param>
        /// <param name="currentAdapter">AdapterEnum that stores the list of DisplayModes.</param>
        /// <returns>An ArrayList of supported Adapter Formats used in the supported DisplayModes.</returns>
        private ArrayList EnumerateDisplayModes( AdapterInformation a, AdapterEnum currentAdapter )
        {
            ArrayList adapterFormatList = new ArrayList();
            for ( int i = 0; i < AdapterFormats.Length; ++i )
            {
                foreach ( DisplayMode d in a.SupportedDisplayModes[ AdapterFormats[ i ] ] )
                {
                    currentAdapter.DisplayModeList.Add( d );

                    // Add Adapter Format used with this DisplayMode
                    if ( !adapterFormatList.Contains( d.Format ) )
                        adapterFormatList.Add( d.Format );
                }
            }

            DisplayModeSorter sorter = new DisplayModeSorter();
            currentAdapter.DisplayModeList.Sort( sorter );
            return adapterFormatList;
        }

        /// <summary>
        /// Enumerates all the devices that an Adapter can make.
        /// </summary>
        /// <param name="currentAdapter">The adapter in question.</param>
        /// <param name="adapterFormatList">An ArrayList of of Adapter Formats supported 
        /// by currentAdapter.</param>
        private void EnumerateDevices( AdapterEnum currentAdapter, ArrayList adapterFormatList )
        {
            // Get all the Devices this adapter can make
            foreach ( DeviceType t in DeviceTypes )
            {
                DeviceEnum currentDevice = new DeviceEnum();

                // Store the DeviceType and Caps
                currentDevice.DeviceType = t;
                try
                {
                    currentDevice.Caps = Manager.GetDeviceCaps( ( int )currentAdapter.AdapterOrdinal,
                        currentDevice.DeviceType );
                }
                catch ( DirectXException )
                {
                    // An exception will be thrown if t == DeviceType.Software. Just ignore it.
                }

                // Find the supported settings for this device
                EnumerateDeviceSettings( currentDevice, currentAdapter, adapterFormatList );

                // Add the device to the list if it has any valid settings
                if ( currentDevice.SettingsList.Count > 0 )
                    currentAdapter.DeviceEnumList.Add( currentDevice );
            }
        }

        /// <summary>
        /// Enumerates possible device settings/configurations for a given device.
        /// </summary>
        private void EnumerateDeviceSettings( DeviceEnum currentDevice, AdapterEnum currentAdapter,
            ArrayList adapterFormatList )
        {
            // Go through each adapter format
            foreach ( Format adapterFormat in AdapterFormats )
            {
                // Go through each back buffer format
                foreach ( Format backBufferFormat in BackBufferFormats )
                {
                    // Check both windowed and fullscreen modes
                    for ( int i = 0; i < 2; ++i )
                    {
                        bool windowed = ( i == 1 );

                        // Skip if this is not a supported Device type
                        if ( !Manager.CheckDeviceType( ( int )currentAdapter.AdapterOrdinal,
                            currentDevice.DeviceType, adapterFormat, backBufferFormat,
                            windowed ) )
                            continue;

                        DeviceSettingsEnum deviceSettings = new DeviceSettingsEnum();

                        // Store the information
                        deviceSettings.AdapterInformation = currentAdapter;
                        deviceSettings.DeviceInformation = currentDevice;
                        deviceSettings.AdapterOrdinal = currentAdapter.AdapterOrdinal;
                        deviceSettings.DeviceType = currentDevice.DeviceType;
                        deviceSettings.AdapterFormat = adapterFormat;
                        deviceSettings.BackBufferFormat = backBufferFormat;
                        deviceSettings.Windowed = windowed;

                        // Create the settings Arrays
                        EnumerateDepthStencilFormats( deviceSettings );
                        EnumerateMultiSampleTypes( deviceSettings );
                        EnumerateVertexProcessingTypes( deviceSettings );
                        EnumeratePresentIntervals( deviceSettings );

                        // Add the settings to the Device's settings list
                        currentDevice.SettingsList.Add( deviceSettings );
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all the depth stencil formats compatible with the given device settings.
        /// </summary>
        /// <param name="deviceSettings"></param>
        private void EnumerateDepthStencilFormats( DeviceSettingsEnum deviceSettings )
        {
            // Check each DepthFormat
            foreach ( DepthFormat depthFormat in DepthFormats )
            {
                // Check if the DepthFormat is a valid surface format
                if ( Manager.CheckDeviceFormat( ( int )deviceSettings.AdapterOrdinal, deviceSettings.DeviceType,
                    deviceSettings.AdapterFormat, Usage.DepthStencil, ResourceType.Surface, depthFormat ) )
                {
                    // Check if the DepthFormat is compatible with the BackBufferFormat
                    if ( Manager.CheckDepthStencilMatch( ( int )deviceSettings.AdapterOrdinal,
                        deviceSettings.DeviceType, deviceSettings.AdapterFormat,
                        deviceSettings.BackBufferFormat, depthFormat ) )
                    {
                        // Add it to the list
                        deviceSettings.DepthStencilFormatList.Add( depthFormat );
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all the compatible multisample types compatible with the given device settings.
        /// </summary>
        private void EnumerateMultiSampleTypes( DeviceSettingsEnum deviceSettings )
        {
            // Check each MultiSampleType
            foreach ( MultiSampleType t in MultiSampleTypes )
            {
                int result, quality;
                // See if it's supported
                if ( Manager.CheckDeviceMultiSampleType( ( int )deviceSettings.AdapterOrdinal,
                    deviceSettings.DeviceType, deviceSettings.BackBufferFormat, deviceSettings.Windowed,
                    t, out result, out quality ) )
                {
                    deviceSettings.MultiSampleTypeList.Add( t );
                    deviceSettings.MultiSampleQualityList.Add( quality );
                }
            }
        }

        /// <summary>
        /// Enumerates all the VertexProcessingTypes compatible with the given device settings.
        /// </summary>
        private void EnumerateVertexProcessingTypes( DeviceSettingsEnum deviceSettings )
        {
            // Best option is stored first
            // Check for hardware T&L
            if ( deviceSettings.DeviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight )
            {
                // Check for pure device
                if ( deviceSettings.DeviceInformation.Caps.DeviceCaps.SupportsPureDevice )
                {
                    deviceSettings.VertexProcessingTypeList.Add( CreateFlags.PureDevice |
                        CreateFlags.HardwareVertexProcessing );
                }
                deviceSettings.VertexProcessingTypeList.Add( CreateFlags.HardwareVertexProcessing );
                deviceSettings.VertexProcessingTypeList.Add( CreateFlags.MixedVertexProcessing );
            }

            // Always supports software
            deviceSettings.VertexProcessingTypeList.Add( CreateFlags.SoftwareVertexProcessing );
        }

        /// <summary>
        /// Enumerates all the present intervals the given device settings support.
        /// </summary>
        private void EnumeratePresentIntervals( DeviceSettingsEnum deviceSettings )
        {
            foreach ( PresentInterval p in PresentIntervals )
            {
                // If the device is windowed, skip all the intervals above one
                if ( deviceSettings.Windowed )
                {
                    if ( ( p == PresentInterval.Two ) || ( p == PresentInterval.Three ) ||
                        ( p == PresentInterval.Four ) )
                    {
                        continue;
                    }
                }

                if ( p == PresentInterval.Default )
                {
                    // Default interval is always available
                    deviceSettings.PresentIntervalList.Add( p );
                }

                // Check if the PresentInterval is supported
                if ( ( deviceSettings.DeviceInformation.Caps.PresentationIntervals & p ) != 0 )
                {
                    deviceSettings.PresentIntervalList.Add( p );
                }
            }
        }
        #endregion
    }
}