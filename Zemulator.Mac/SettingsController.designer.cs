// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Zemulator.Mac
{
	[Register ("SettingsController")]
	partial class SettingsController
	{
		[Outlet]
		AppKit.NSPopUpButton LabelHeightButton { get; set; }

		[Outlet]
		AppKit.NSPopUpButton LabelWidthButton { get; set; }

		[Outlet]
		AppKit.NSPopUpButton PrintDensityButton { get; set; }

		[Action ("LabelHeightChanged:")]
		partial void LabelHeightChanged (Foundation.NSObject sender);

		[Action ("LabelWidthChanged:")]
		partial void LabelWidthChanged (Foundation.NSObject sender);

		[Action ("PrintDensityChanged:")]
		partial void PrintDensityChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (LabelWidthButton != null) {
				LabelWidthButton.Dispose ();
				LabelWidthButton = null;
			}

			if (LabelHeightButton != null) {
				LabelHeightButton.Dispose ();
				LabelHeightButton = null;
			}

			if (PrintDensityButton != null) {
				PrintDensityButton.Dispose ();
				PrintDensityButton = null;
			}
		}
	}
}
