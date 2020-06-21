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
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSStackView LabelStack { get; set; }

		[Outlet]
		MyView LabelView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LabelStack != null) {
				LabelStack.Dispose ();
				LabelStack = null;
			}
		}
	}
}
