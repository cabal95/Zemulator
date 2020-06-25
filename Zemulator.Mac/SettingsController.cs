using System;
using System.Collections.Generic;
using System.Linq;

using AppKit;

using Foundation;

using Zemulator.Common;

namespace Zemulator.Mac
{
    /// <summary>
    /// Handles the Settings window view.
    /// </summary>
	public partial class SettingsController : NSViewController
	{
        #region Setting Keys

        /// <summary>
        /// The width, in inches, of the labels.
        /// </summary>
        public const string LabelWidthKey = "LabelWidth";

        /// <summary>
        /// The height, in inches, of the labels.
        /// </summary>
        public const string LabelHeightKey = "LabelHeight";

        /// <summary>
        /// The density (DPI) used when generating labels.
        /// </summary>
        public const string PrintDensityKey = "PrintDensity";

        #endregion

        #region Fields

        /// <summary>
        /// The list of label sizes we support.
        /// </summary>
        private readonly List<Item<double>> _labelSizes;

        /// <summary>
        /// The label densities we support.
        /// </summary>
        private readonly List<Item<LabelDensity>> _labelDensities;

        #endregion

        #region Constructors

        /// <summary>
        /// Creats a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="handle">The native handle that identifies this view controller.</param>
        public SettingsController (IntPtr handle) : base (handle)
		{
            _labelSizes = new List<Item<double>>();
            for ( double size = 2.0d; size <= 6.0d; size += 0.25d )
            {
                _labelSizes.Add( new Item<double>( size, size.ToString( "F2" ) + "\"" ) );
            }

            _labelDensities = new List<Item<LabelDensity>>
            {
                new Item<LabelDensity>( LabelDensity.Density_152, "152dpi" ),
                new Item<LabelDensity>( LabelDensity.Density_203, "203dpi" ),
                new Item<LabelDensity>( LabelDensity.Density_300, "300dpi" ),
                new Item<LabelDensity>( LabelDensity.Density_600, "600dpi" )
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// The view has loaded and will shortly be displayed.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var defaults = NSUserDefaults.StandardUserDefaults;

            //
            // Set initial field values.
            //
            BindPopupItems( LabelWidthButton, _labelSizes, defaults.DoubleForKey( LabelWidthKey ) );
            BindPopupItems( LabelHeightButton, _labelSizes, defaults.DoubleForKey( LabelHeightKey ) );
            BindPopupItems( PrintDensityButton, _labelDensities, ( LabelDensity ) ( int ) defaults.IntForKey( PrintDensityKey ) );
        }

        /// <summary>
        /// Populates the PopUpButton's item list from those specified and
        /// then selects the default value.
        /// </summary>
        /// <typeparam name="T">The type of item being added.</typeparam>
        /// <param name="button">The button to be populated.</param>
        /// <param name="items">The list of items.</param>
        /// <param name="defaultValue">The default value to be selected.</param>
        private void BindPopupItems<T>( NSPopUpButton button, ICollection<Item<T>> items, T defaultValue )
            where T : IComparable
        {
            button.RemoveAllItems();

            foreach ( var item in items )
            {
                button.AddItem( item.Title );
            }

            var defaultItem = items.SingleOrDefault( a => a.Value.CompareTo( defaultValue ) == 0 );

            if ( defaultItem != null )
            {
                button.SelectItem( defaultItem.Title );
            }
            else
            {
                button.SelectItem( 0 );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the selected value of the LabelWidthButton changes.
        /// </summary>
        /// <param name="sender">The object that sent this message.</param>
        partial void LabelWidthChanged( NSObject sender )
        {
            var size = _labelSizes[( int ) LabelWidthButton.IndexOfSelectedItem];

            NSUserDefaults.StandardUserDefaults.SetDouble( size.Value, LabelWidthKey );
        }

        /// <summary>
        /// Called when the selected value of the LabelHeightButton changes.
        /// </summary>
        /// <param name="sender">The object that sent this message.</param>
        partial void LabelHeightChanged( NSObject sender )
        {
            var size = _labelSizes[( int ) LabelHeightButton.IndexOfSelectedItem];

            NSUserDefaults.StandardUserDefaults.SetDouble( size.Value, LabelHeightKey );
        }

        /// <summary>
        /// Called when the selected value of the PrintDensityButton changes.
        /// </summary>
        /// <param name="sender">The object that sent this message.</param>
        partial void PrintDensityChanged( NSObject sender )
        {
            var density = _labelDensities[( int ) PrintDensityButton.IndexOfSelectedItem];

            NSUserDefaults.StandardUserDefaults.SetInt( ( int ) density.Value, PrintDensityKey );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Helper class to more easily allow us to work with popup buttons.
        /// </summary>
        /// <typeparam name="T">The type of value stored.</typeparam>
        private class Item<T>
        {
            /// <summary>
            /// The value stored in this item.
            /// </summary>
            public T Value { get; }

            /// <summary>
            /// The user friendly text that identifies this item.
            /// </summary>
            public string Title { get; }

            /// <summary>
            /// Creates an item instance.
            /// </summary>
            /// <param name="value">The value to be stored with the item.</param>
            /// <param name="title">The user friendly text to identify this item.</param>
            public Item( T value, string title )
            {
                Value = value;
                Title = title;
            }
        }

        #endregion
    }
}
