using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GamePrototype
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            Gas.Helpers.Config config = new Gas.Helpers.Config( "Config.txt" );

            using ( GamePrototypeForm form = new GamePrototypeForm() )
            {
                form.Run( config.GetSetting<bool>( "Windowed" ),
                    config.GetSetting<int>( "DesiredWidth" ),
                    config.GetSetting<int>( "DesiredHeight" ),
                    config.GetSetting<string>( "WindowTitle" ) );
            }
        }
    }
}