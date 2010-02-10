//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright 2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#pragma warning disable 67 // "The event 'E' is never used" shown for CleanUpVirtualizedItemEvent

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;

namespace System.Windows.Controls {
	public class VirtualizingStackPanel : VirtualizingPanel, IScrollInfo
	{
		static readonly double LineDelta = 14.7;
		static readonly double Wheelitude = 3;
		
		//
		// DependencyProperties
		//
		public static readonly DependencyProperty IsVirtualizingProperty =
			DependencyProperty.RegisterAttached ("IsVirtualizing",
							     typeof (bool),
							     typeof (VirtualizingStackPanel),
							     new PropertyMetadata (false));
		
		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register ("Orientation",
						     typeof (Orientation),
						     typeof (VirtualizingStackPanel),
						     new PropertyMetadata (Orientation.Vertical));
		
		public static readonly DependencyProperty VirtualizationModeProperty =
			DependencyProperty.RegisterAttached ("VirtualizationMode",
							     typeof (VirtualizationMode),
							     typeof (VirtualizingStackPanel),
							     new PropertyMetadata (VirtualizationMode.Recycling));
		
		
		//
		// Attached Property Accessor Methods
		//
		public static bool GetIsVirtualizing (DependencyObject o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
			
			return (bool) o.GetValue (VirtualizingStackPanel.IsVirtualizingProperty);
		}
		
		public static VirtualizationMode GetVirtualizationMode (DependencyObject element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			
			return (VirtualizationMode) element.GetValue (VirtualizingStackPanel.VirtualizationModeProperty);
		}
		
		public static void SetVirtualizationMode (DependencyObject element, VirtualizationMode mode)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			
			element.SetValue (VirtualizingStackPanel.VirtualizationModeProperty, mode);
		}
		
		
		//
		// Property Accessors
		//
		public Orientation Orientation {
			get { return (Orientation) GetValue (VirtualizingStackPanel.OrientationProperty); }
			set { SetValue (VirtualizingStackPanel.OrientationProperty, value); }
		}
		
		
		//
		// Constructors
		//
		public VirtualizingStackPanel ()
		{
		}
		
		
		//
		// Helper Methods
		//
		void RemoveUnusedContainers (int first, int count)
		{
			IRecyclingItemContainerGenerator generator = ItemContainerGenerator as IRecyclingItemContainerGenerator;
			ItemsControl owner = ItemsControl.GetItemsOwner (this);
			VirtualizationMode mode = GetVirtualizationMode (this);
			CleanUpVirtualizedItemEventArgs args;
			int last = first + count - 1;
			
			//Console.WriteLine ("VSP.RemoveUnusedContainers ({0}, {1});", first, count);
			
			for (int i = Children.Count - 1; i >= 0; i--) {
				GeneratorPosition pos = new GeneratorPosition (i, 0);
				int item = generator.IndexFromGeneratorPosition (pos);
				
				if (item < first || item > last) {
					//Console.WriteLine ("\tRemoving item[{0}] (child #{1})", item, i);
					args = new CleanUpVirtualizedItemEventArgs (Children[i], owner.Items[item]);
					OnCleanUpVirtualizedItem (args);
					RemoveInternalChildRange (i, 1);
					
					if (!args.Cancel) {
						if (mode == VirtualizationMode.Recycling)
							generator.Recycle (pos, 1);
						else
							generator.Remove (pos, 1);
					}
				}
			}
		}
		
		
		//
		// Method Overrides
		//
		protected override Size MeasureOverride (Size availableSize)
		{
			ItemsControl owner = ItemsControl.GetItemsOwner (this);
			Size measured = new Size (0, 0);
			bool invalidate = false;
			int nvisible = 0;
			int beyond = 0;
			int index;
			
			if (Orientation == Orientation.Horizontal)
				index = (int) HorizontalOffset;
			else
				index = (int) VerticalOffset;
			
			if (owner.Items.Count > 0) {
				IItemContainerGenerator generator = ItemContainerGenerator;
				GeneratorPosition start;
				Size childAvailable;
				int insertAt;
				
				// Calculate the child sizing constraints
				childAvailable = new Size (double.PositiveInfinity, double.PositiveInfinity);
				
				if (Orientation == Orientation.Vertical) {
					// Vertical layout
					if (!Double.IsNaN (this.Width))
						childAvailable.Width = this.Width;
					
					childAvailable.Width = Math.Min (childAvailable.Width, this.MaxWidth);
					childAvailable.Width = Math.Max (childAvailable.Width, this.MinWidth);
				} else {
					// Horizontal layout
					if (!Double.IsNaN (this.Height))
						childAvailable.Height = this.Height;
					
					childAvailable.Height = Math.Min (childAvailable.Height, this.MaxHeight);
					childAvailable.Height = Math.Max (childAvailable.Height, this.MinHeight);
				}
				
				// Next, prepare and measure the extents of our viewable items...
				start = generator.GeneratorPositionFromIndex (index);
				insertAt = (start.Offset == 0) ? start.Index : start.Index + 1;
				
				using (generator.StartAt (start, GeneratorDirection.Forward, true)) {
					bool isNewlyRealized;
					
					for (int i = index; i < owner.Items.Count && beyond < 2; i++, insertAt++) {
						// Generate the child container
						UIElement child = generator.GenerateNext (out isNewlyRealized) as UIElement;
						if (isNewlyRealized) {
							// Add newly created children to the panel
							if (insertAt < Children.Count)
								InsertInternalChild (insertAt, child);
							else
								AddInternalChild (child);
						}
						
						generator.PrepareItemContainer (child);
						
						// Call Measure() on the child to both force layout and also so
						// that we can figure out when to stop adding children (e.g. when
						// we go beyond the viewable area)
						child.Measure (childAvailable);
						Size size = child.DesiredSize;
						
						nvisible++;
						
						if (Orientation == Orientation.Vertical) {
							measured.Width = Math.Max (measured.Width, size.Width);
							measured.Height += size.Height;
							
							if (measured.Height > availableSize.Height)
								beyond++;
						} else {
							measured.Height = Math.Max (measured.Height, size.Height);
							measured.Width += size.Width;
							
							if (measured.Width > availableSize.Width)
								beyond++;
						}
					}
				}
			}
			
			if (nvisible > 0)
				RemoveUnusedContainers (index, nvisible);
			
			nvisible -= beyond;
			
			// Update our Extent and Viewport values
			if (Orientation == Orientation.Vertical) {
				if (ExtentHeight != owner.Items.Count) {
					ExtentHeight = owner.Items.Count;
					invalidate = true;
				}
				
				if (ExtentWidth != measured.Width) {
					ExtentWidth = measured.Width;
					invalidate = true;
				}
				
				if (ViewportHeight != nvisible) {
					ViewportHeight = nvisible;
					invalidate = true;
				}
				
				if (ViewportWidth != availableSize.Width) {
					ViewportWidth = availableSize.Width;
					invalidate = true;
				}
				
				// Massage 'measured' into what Silverlight would return...
				measured.Width = Math.Min (measured.Width, availableSize.Width);
				if (!Double.IsPositiveInfinity (availableSize.Height))
					measured.Height = availableSize.Height;
			} else {
				if (ExtentHeight != measured.Height) {
					ExtentHeight = measured.Height;
					invalidate = true;
				}
				
				if (ExtentWidth != owner.Items.Count) {
					ExtentWidth = owner.Items.Count;
					invalidate = true;
				}
				
				if (ViewportHeight != availableSize.Height) {
					ViewportHeight = availableSize.Height;
					invalidate = true;
				}
				
				if (ViewportWidth != nvisible) {
					ViewportWidth = nvisible;
					invalidate = true;
				}
				
				// Massage 'measured' into what Silverlight would return...
				measured.Height = Math.Min (measured.Height, availableSize.Height);
				if (!Double.IsPositiveInfinity (availableSize.Width))
					measured.Width = availableSize.Width;
			}
			
			if (invalidate && ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
			
			return measured;
		}
		
		protected override Size ArrangeOverride (Size finalSize)
		{
			ItemsControl owner = ItemsControl.GetItemsOwner (this);
			Size arranged = finalSize;
			
			if (Orientation == Orientation.Vertical)
				arranged.Height = 0;
			else
				arranged.Width = 0;
			
			// Arrange our children
			foreach (UIElement child in Children) {
				Size size = child.DesiredSize;
				
				if (Orientation == Orientation.Vertical) {
					Rect childFinal = new Rect (-HorizontalOffset, arranged.Height, size.Width, size.Height);
					
					if (childFinal.IsEmpty)
						child.Arrange (new Rect ());
					else
						child.Arrange (childFinal);
					
					arranged.Width = Math.Max (arranged.Width, size.Width);
					arranged.Height += size.Height;
				} else {
					Rect childFinal = new Rect (arranged.Width, -VerticalOffset, size.Width, size.Height);
					
					if (childFinal.IsEmpty)
						child.Arrange (new Rect ());
					else
						child.Arrange (childFinal);
					
					arranged.Width += size.Width;
					arranged.Height = Math.Max (arranged.Height, size.Height);
				}
			}
			
			if (Orientation == Orientation.Vertical)
				arranged.Height = Math.Max (arranged.Height, finalSize.Height);
			else
				arranged.Width = Math.Max (arranged.Width, finalSize.Width);
			
			return arranged;
		}
		
		protected override void OnClearChildren ()
		{
			base.OnClearChildren ();
			
			HorizontalOffset = 0;
			VerticalOffset = 0;
			ViewportHeight = 0;
			ViewportWidth = 0;
			ExtentHeight = 0;
			ExtentWidth = 0;
			
			InvalidateMeasure ();
			
			if (ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
		}
		
 		protected override void OnItemsChanged (object sender, ItemsChangedEventArgs args)
 		{
			IItemContainerGenerator generator = ItemContainerGenerator;
			ItemsControl owner = ItemsControl.GetItemsOwner (this);
			int index, offset, viewable, delta;
			
			switch (args.Action) {
			case NotifyCollectionChangedAction.Add:
				// The following logic is meant to keep the current viewable items in view
				// after adjusting for added items.
				index = generator.IndexFromGeneratorPosition (args.Position);
				if (Orientation == Orientation.Horizontal)
					offset = (int) HorizontalOffset;
				else
					offset = (int) VerticalOffset;
				
				if (index <= offset) {
					// items have been added earlier in the list than what is viewable
					offset += args.ItemCount;
				}
				
				if (Orientation == Orientation.Horizontal)
					HorizontalOffset = offset;
				else
					VerticalOffset = offset;
				break;
			case NotifyCollectionChangedAction.Remove:
				// The following logic is meant to keep the current viewable items in view
				// after adjusting for removed items.
				index = generator.IndexFromGeneratorPosition (args.Position);
				if (Orientation == Orientation.Horizontal) {
					offset = (int) HorizontalOffset;
					viewable = (int) ViewportWidth;
				} else {
					viewable = (int) ViewportHeight;
					offset = (int) VerticalOffset;
				}
				
				if (index < offset) {
					// items earlier in the list than what is viewable have been removed
					offset = Math.Max (offset - args.ItemCount, 0);
				}
				
				// adjust for items removed in the current view and/or beyond the current view
				offset = Math.Min (offset, owner.Items.Count - viewable);
				offset = Math.Max (offset, 0);
				
				if (Orientation == Orientation.Horizontal)
					HorizontalOffset = offset;
				else
					VerticalOffset = offset;
				
				RemoveInternalChildRange (args.Position.Index, args.ItemUICount);
				break;
			case NotifyCollectionChangedAction.Replace:
				RemoveInternalChildRange (args.Position.Index, args.ItemUICount);
				break;
			case NotifyCollectionChangedAction.Reset:
				OnClearChildren ();
				return;
			}
			
			InvalidateMeasure ();
			
			if (ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
 		}
		
		
		//
		// Methods
		//
		protected virtual void OnCleanUpVirtualizedItem (CleanUpVirtualizedItemEventArgs e)
		{
			CleanUpVirtualizedItemEventHandler handler = CleanUpVirtualizedItemEvent;
			
			if (handler != null)
				handler (this, e);
		}
		
#region "IScrollInfo"
		public bool CanHorizontallyScroll { get; set; }
		public bool CanVerticallyScroll { get; set; }
		
		public double ExtentWidth {
			get; private set;
		}
		
		public double ExtentHeight {
			get; private set;
		}
		
		public double HorizontalOffset {
			get; private set;
		}
		
		public double VerticalOffset {
			get; private set;
		}
		
		public double ViewportWidth {
			get; private set;
		}
		
		public double ViewportHeight {
			get; private set;
		}
		
		public ScrollViewer ScrollOwner { get; set; }
		
		//
		// Note: When scrolling along the stacking orientation,
		// Silverlight will perform logical scrolling. That
		// is, to say, it will scroll by items and not pixels.
		//
		
		public virtual void LineDown ()
		{
			if (Orientation == Orientation.Horizontal)
				SetVerticalOffset (VerticalOffset + LineDelta);
			else
				SetVerticalOffset (VerticalOffset + 1);
		}
		
		public virtual void LineLeft ()
		{
			if (Orientation == Orientation.Vertical)
				SetHorizontalOffset (HorizontalOffset - LineDelta);
			else
				SetHorizontalOffset (HorizontalOffset - 1);
		}
		
		public virtual void LineRight ()
		{
			if (Orientation == Orientation.Vertical)
				SetHorizontalOffset (HorizontalOffset + LineDelta);
			else
				SetHorizontalOffset (HorizontalOffset + 1);
		}
		
		public virtual void LineUp ()
		{
			if (Orientation == Orientation.Horizontal)
				SetVerticalOffset (VerticalOffset - LineDelta);
			else
				SetVerticalOffset (VerticalOffset - 1);
		}
		
		public virtual Rect MakeVisible (UIElement visual, Rect rectangle)
		{
			Rect exposed = new Rect (0, 0, 0, 0);
			
			foreach (UIElement child in Children) {
				if (child == visual) {
					if (Orientation == Orientation.Vertical) {
						if (rectangle.X != HorizontalOffset)
							SetHorizontalOffset (rectangle.X);
						
						exposed.Width = Math.Min (child.RenderSize.Width, ViewportWidth);
						exposed.Height = child.RenderSize.Height;
						exposed.X = HorizontalOffset;
					} else {
						if (rectangle.Y != VerticalOffset)
							SetVerticalOffset (rectangle.Y);
						
						exposed.Height = Math.Min (child.RenderSize.Height, ViewportHeight);
						exposed.Width = child.RenderSize.Width;
						exposed.Y = VerticalOffset;
					}
					
					return exposed;
				}
				
				if (Orientation == Orientation.Vertical)
					exposed.Y += child.RenderSize.Height;
				else
					exposed.X += child.RenderSize.Width;
			}
			
			throw new ArgumentException ("Visual is not a child of this Panel");
		}
		
		public virtual void MouseWheelDown ()
		{
			if (Orientation == Orientation.Horizontal)
				SetVerticalOffset (VerticalOffset + LineDelta * Wheelitude);
			else
				SetVerticalOffset (VerticalOffset + Wheelitude);
		}
		
		public virtual void MouseWheelLeft ()
		{
			if (Orientation == Orientation.Vertical)
				SetHorizontalOffset (HorizontalOffset - LineDelta * Wheelitude);
			else
				SetHorizontalOffset (HorizontalOffset - Wheelitude);
		}
		
		public virtual void MouseWheelRight ()
		{
			if (Orientation == Orientation.Vertical)
				SetHorizontalOffset (HorizontalOffset + LineDelta * Wheelitude);
			else
				SetHorizontalOffset (HorizontalOffset + Wheelitude);
		}
		
		public virtual void MouseWheelUp ()
		{
			if (Orientation == Orientation.Horizontal)
				SetVerticalOffset (VerticalOffset - LineDelta * Wheelitude);
			else
				SetVerticalOffset (VerticalOffset - Wheelitude);
		}
		
		public virtual void PageDown ()
		{
			SetVerticalOffset (VerticalOffset + ViewportHeight);
		}
		
		public virtual void PageLeft ()
		{
			SetHorizontalOffset (HorizontalOffset - ViewportWidth);
		}
		
		public virtual void PageRight ()
		{
			SetHorizontalOffset (HorizontalOffset + ViewportWidth);
		}
		
		public virtual void PageUp ()
		{
			SetVerticalOffset (VerticalOffset - ViewportHeight);
		}
		
		public void SetHorizontalOffset (double offset)
		{
			if (offset < 0 || ViewportWidth >= ExtentWidth)
				offset = 0;
			else if (offset + ViewportWidth >= ExtentWidth)
				offset = ExtentWidth - ViewportWidth;
			
			if (HorizontalOffset == offset)
				return;
			
			HorizontalOffset = offset;
			
			if (Orientation == Orientation.Horizontal)
				InvalidateMeasure ();
			else
				InvalidateArrange ();
			
			if (ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
		}
		
		public void SetVerticalOffset (double offset)
		{
			if (offset < 0 || ViewportHeight >= ExtentHeight)
				offset = 0;
			else if (offset + ViewportHeight >= ExtentHeight)
				offset = ExtentHeight - ViewportHeight;
			
			if (VerticalOffset == offset)
				return;
			
			VerticalOffset = offset;
			
			if (Orientation == Orientation.Vertical)
				InvalidateMeasure ();
			else
				InvalidateArrange ();
			
			if (ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
		}
#endregion "IScrollInfo"
		
		
		//
		// Events
		//
		public event CleanUpVirtualizedItemEventHandler CleanUpVirtualizedItemEvent;
	}

}
