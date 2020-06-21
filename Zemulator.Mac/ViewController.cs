using System;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace Zemulator.Mac
{
    public partial class ViewController : NSViewController
    {
        public ViewController( IntPtr handle ) : base( handle )
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var datasource = new TableDataSource<string>();
            datasource.Add( "hello" );
            TableView.DataSource = datasource;
            TableView.Delegate = new LabelTableDelegate( datasource );
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

        private TableDataSource<string> _dataSource;

        public LabelTableDelegate( TableDataSource<string> dataSource )
        {
            _dataSource = dataSource;
        }

        public override NSView GetViewForItem( NSTableView tableView, NSTableColumn tableColumn, nint row )
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data
            NSTextField view = ( NSTextField ) tableView.MakeView( CellIdentifier, this );
            if ( view == null )
            {
                view = new NSTextField();
                view.Identifier = CellIdentifier;
                view.BackgroundColor = NSColor.Clear;
                view.Bordered = false;
                view.Selectable = false;
                view.Editable = false;
            }

            // Setup view based on the column selected
            switch ( tableColumn.Title )
            {
                case "Label":
                    view.StringValue = _dataSource[( int ) row];
                    break;
            }

            return view;
        }
    }
}
