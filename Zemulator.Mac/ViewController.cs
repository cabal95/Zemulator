using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using CoreGraphics;

namespace Zemulator.Mac
{
    [Register( "MyView" )]
    public class MyView : NSView
    {
        public override bool IsFlipped => true;

        protected int LabelPadding => 12;

        public MyView() : base()
        {
        }

        public override void SetFrameSize( CGSize newSize )
        {
            base.SetFrameSize( new CGSize( newSize.Width, GetHeightForWidth( newSize.Width ) ) );
        }

        private nfloat GetHeightForWidth( nfloat targetWidth )
        {
            nfloat y = LabelPadding;
            var scaleFactor = Window.BackingScaleFactor;

            foreach ( var sv in Subviews )
            {
                if ( sv is NSImageView iv )
                {
                    var width = Math.Min( targetWidth - ( LabelPadding * 2 ), iv.Image.Size.Width / scaleFactor );
                    var ratio = iv.Image.Size.Height / iv.Image.Size.Width;

                    y += ( nfloat ) ( width * ratio ) + LabelPadding;
                }
            }

            return y;
        }

        public override void Layout()
        {
            nfloat y = LabelPadding;
            var scaleFactor = Window.BackingScaleFactor;
            var targetWidth = Frame.Size.Width;

            foreach ( var sv in Subviews )
            {
                if ( sv is NSImageView iv )
                {
                    var width = Math.Min( targetWidth - ( LabelPadding * 2 ), iv.Image.Size.Width / scaleFactor );
                    var ratio = iv.Image.Size.Height / iv.Image.Size.Width;

                    iv.Frame = new CGRect( ( targetWidth - width ) / 2.0, y, width, width * ratio );

                    y += iv.Frame.Size.Height + LabelPadding;
                }
            }
        }
    }


    [Register( "CheckerView" )]
    public class CheckerView : FlippedView
    {
        public CheckerView( IntPtr handle ) : base( handle )
        {
        }

        public override void DrawRect( CGRect dirtyRect )
        {
            const int boxSize = 16;
            var colors = new[] { NSColor.DarkGray, NSColor.LightGray };
            int colorIndex;
            int yIndex = 0;

            for ( int y = 0; y < Frame.Size.Height; y += boxSize, yIndex += 1 )
            {
                colorIndex = yIndex & 1;

                for ( int x = 0, xIndex = 0; x < Frame.Size.Width; x += boxSize, xIndex += 1 )
                {
                    var color = colors[( xIndex + colorIndex ) & 1];

                    color.Set();
                    NSBezierPath.FillRect( new CGRect( x, y, boxSize, boxSize ) );
                }
            }
        }
    }

    public class FlippedView : NSView
    {
        public override bool IsFlipped => true;

        public FlippedView( IntPtr handle ) : base( handle )
        {
        }
    }

    public class FlippedClipView : NSClipView
    {
        public override bool IsFlipped => true;
    }

    public partial class ViewController : NSViewController
    {
        public ViewController( IntPtr handle ) : base( handle )
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var sv = new NSScrollView( View.Bounds );
            sv.TranslatesAutoresizingMaskIntoConstraints = false;
            sv.DrawsBackground = false;
            sv.HasVerticalScroller = true;
            View.AddSubview( sv );
            View.AddConstraint( sv.WidthAnchor.ConstraintEqualToAnchor( View.WidthAnchor ) );
            View.AddConstraint( sv.HeightAnchor.ConstraintEqualToAnchor( View.HeightAnchor ) );

            var clipView = new FlippedClipView();
            clipView.TranslatesAutoresizingMaskIntoConstraints = false;
            clipView.DrawsBackground = false;
            sv.ContentView = clipView;
            sv.AddConstraint( clipView.LeftAnchor.ConstraintEqualToAnchor( sv.LeftAnchor ) );
            sv.AddConstraint( clipView.TopAnchor.ConstraintEqualToAnchor( sv.TopAnchor ) );
            sv.AddConstraint( clipView.RightAnchor.ConstraintEqualToAnchor( sv.RightAnchor ) );
            sv.AddConstraint( clipView.BottomAnchor.ConstraintEqualToAnchor( sv.BottomAnchor ) );

            var mv = new MyView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            sv.DocumentView = mv;
            clipView.AddConstraint( clipView.LeftAnchor.ConstraintEqualToAnchor( mv.LeftAnchor ) );
            clipView.AddConstraint( clipView.TopAnchor.ConstraintEqualToAnchor( mv.TopAnchor ) );
            clipView.AddConstraint( clipView.RightAnchor.ConstraintEqualToAnchor( mv.RightAnchor ) );

            var zplImage = new NSImage( new NSUrl( "https://nyc3.digitaloceanspaces.com/aph/app/uploads/2019/04/26160704/1-04851-00_BL_Notebook_Paper_Punch_G.jpg" ) );
            var v1 = new NSImageView
            {
                Image = zplImage,
                ImageAlignment = NSImageAlignment.Center,
                ImageScaling = NSImageScale.ProportionallyUpOrDown,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mv.AddSubview( v1 );

            zplImage = new NSImage( new NSUrl( "https://nyc3.digitaloceanspaces.com/aph/app/uploads/2019/04/26160704/1-04851-00_BL_Notebook_Paper_Punch_G.jpg" ) );
            var v2 = new NSImageView
            {
                Image = zplImage,
                ImageAlignment = NSImageAlignment.Center,
                ImageScaling = NSImageScale.ProportionallyUpOrDown,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mv.AddSubview( v2 );
            //var c1 = v1.WidthAnchor.ConstraintEqualToConstant( 500 );
            //c1.Priority = 50;
            //var c2 = NSLayoutConstraint.Create( v1, NSLayoutAttribute.Width, NSLayoutRelation.LessThanOrEqual, mv, NSLayoutAttribute.Width, 1, -40 );
            //c2.Priority = 249;
            //var c3 = NSLayoutConstraint.Create( v1, NSLayoutAttribute.Height, NSLayoutRelation.Equal, v1, NSLayoutAttribute.Width, 1, 0 );
            //c3.Priority = 100;
            //            mv.AddConstraints( new[] { c1, c2, c3 } );
            //            mv.AddConstraint( v1.CenterXAnchor.ConstraintEqualToAnchor( mv.CenterXAnchor ) );
            //            mv.AddConstraint( v1.TopAnchor.ConstraintEqualToAnchor( mv.TopAnchor ) );

            //            mv.AddConstraint( mv.BottomAnchor.ConstraintEqualToAnchor( v1.BottomAnchor ) );
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        [Action( "openSettings:" )]
        public void OpenSettings( NSObject sender )
        {
        }

        [Action( "clearLabels:" )]
        public void ClearLabels( NSObject sender )
        {
        }
    }

    public class TableDataSource<T> : NSTableViewDataSource
    {
        private List<T> _items = new List<T>();

        public override nint GetRowCount( NSTableView tableView )
        {
            return _items.Count;
        }

        public T this[int i]
        {
            get => _items[i];
        }

        public void Add( T item )
        {
            _items.Add( item );
        }
    }

    public class LabelTableDelegate : NSTableViewDelegate
    {
        private const string CellIdentifier = "LabelCell";

        private TableDataSource<NSImage> _dataSource;

        public LabelTableDelegate( TableDataSource<NSImage> dataSource )
        {
            _dataSource = dataSource;
        }

        public override NSView GetViewForItem( NSTableView tableView, NSTableColumn tableColumn, nint row )
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data
            var view = ( NSImageView ) tableView.MakeView( CellIdentifier, this );
            if ( view == null )
            {
                view = new NSImageView();
                view.Identifier = CellIdentifier;
            }

            // Setup view based on the column selected
            switch ( tableColumn.Title )
            {
                case "Label":
                    view.Image = _dataSource[( int ) row];
                    break;
            }

            return view;
        }
    }
}
