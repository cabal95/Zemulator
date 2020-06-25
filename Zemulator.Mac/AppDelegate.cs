using System.Collections.Generic;
using System.Linq;

using AppKit;

using Foundation;

namespace Zemulator.Mac
{
    [Register( "AppDelegate" )]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
            //
            // Set our initial user defaults.
            //
            var defaults = new Dictionary<string, object>
            {
                { SettingsController.LabelWidthKey, 4.00 },
                { SettingsController.LabelHeightKey, 2.00 },
                { SettingsController.PrintDensityKey, Common.LabelDensity.Density_203 }
            };

            NSUserDefaults.StandardUserDefaults.RegisterDefaults( NSDictionary.FromObjectsAndKeys( defaults.Values.ToArray(), defaults.Keys.ToArray() ) );
        }
    }
}
