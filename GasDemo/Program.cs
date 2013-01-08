using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GasDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Properties.Settings.Default.Reload();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            using ( GasDemoForm form = new GasDemoForm() )
            {
                form.Run( form.Config.GetSetting<bool>( "Windowed" ),
                    form.Config.GetSetting<int>( "DesiredWidth" ),
                    form.Config.GetSetting<int>( "DesiredHeight" ),
                    form.Config.GetSetting<string>( "WindowTitle" ) );
            }
        }
    }
}