﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for an item in a DataGrid
    /// </summary>
    sealed public class DataGridItemAutomationPeer : FrameworkElementAutomationPeer,
        IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, ISelectionProvider
    {
        #region Data

        private object _item;
        private AutomationPeer _dataGridAutomationPeer;

        #endregion

        #region Constructors

        /// <summary>
        /// AutomationPeer for an item in a DataGrid
        /// </summary>
        public DataGridItemAutomationPeer(object item, DataGrid dataGrid)
            : base(dataGrid)
        {
            if (item == null)
            {
                // 

                throw new ElementNotAvailableException();
            }
            if (dataGrid == null)
            {
                // 

                throw new ElementNotAvailableException();
            }

            _item = item;
            _dataGridAutomationPeer = FrameworkElementAutomationPeer.CreatePeerForElement(dataGrid);
        }

        #endregion

        #region Properties

        private DataGrid OwningDataGrid
        {
            get
            {
                DataGridAutomationPeer gridPeer = _dataGridAutomationPeer as DataGridAutomationPeer;
                return (DataGrid)gridPeer.Owner;
            }
        }

        private DataGridRow OwningRow
        {
            get
            {
                int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
                if (this.OwningDataGrid.DisplayData.IsRowDisplayed(index))
                {
                    return this.OwningDataGrid.DisplayData.GetDisplayedRow(index);
                }
                return null;

            }
        }

        internal DataGridRowAutomationPeer OwningRowPeer
        {
            get
            {
                DataGridRowAutomationPeer rowPeer = null;
                DataGridRow row = this.OwningRow;
                if (row != null)
                {
                    rowPeer = FrameworkElementAutomationPeer.CreatePeerForElement(row) as DataGridRowAutomationPeer;
                }
                return rowPeer;
            }
        }

        #endregion

        #region AutomationPeer Overrides

        ///
        protected override string GetAcceleratorKeyCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetAcceleratorKey() : string.Empty;
        }

        ///
        protected override string GetAccessKeyCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetAccessKey() : string.Empty;
        }

        ///
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
        }

        ///
        protected override string GetAutomationIdCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetAutomationId() : string.Empty;
        }

        ///
        protected override Rect GetBoundingRectangleCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetBoundingRectangle() : new Rect();
        }

        ///
        protected override List<AutomationPeer> GetChildrenCore()
        {
            if (this.OwningRowPeer != null)
            {
                this.OwningRowPeer.InvalidatePeer();
                return this.OwningRowPeer.GetChildren();
            }
            return new List<AutomationPeer>();
        }

        ///
        protected override string GetClassNameCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetClassName() : string.Empty;
        }

        ///
        protected override Point GetClickablePointCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetClickablePoint() : new Point(double.NaN, double.NaN);
        }

        ///
        protected override string GetHelpTextCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetHelpText() : string.Empty;
        }

        ///
        protected override string GetItemStatusCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetItemStatus() : string.Empty;
        }

        ///
        override protected string GetItemTypeCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetItemType() : string.Empty;
        }

        ///
        protected override AutomationPeer GetLabeledByCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetLabeledBy() : null;
        }

        ///
        protected override string GetLocalizedControlTypeCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetLocalizedControlType() : string.Empty;
        }

        ///
        protected override string GetNameCore()
        {
            return _item.ToString();
        }

        ///
        protected override AutomationOrientation GetOrientationCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetOrientation() : AutomationOrientation.None;
        }

        ///
        public override object GetPattern(PatternInterface patternInterface)
        {
            switch (patternInterface)
            {
                case PatternInterface.Invoke:
                    {
                        if (!this.OwningDataGrid.IsReadOnly)
                        {
                            return this;
                        }
                        break;
                    }
                case PatternInterface.ScrollItem:
                    {
                        if (this.OwningDataGrid.VerticalScrollBar != null &&
                            this.OwningDataGrid.VerticalScrollBar.Maximum > 0)
                        {
                            return this;
                        }
                        break;
                    }
                case PatternInterface.Selection:
                case PatternInterface.SelectionItem:
                    return this;
            }
            return null;
        }

        ///
        protected override bool HasKeyboardFocusCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.HasKeyboardFocus() : false;
        }

        ///
        protected override bool IsContentElementCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsContentElement() : true;
        }

        ///
        protected override bool IsControlElementCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsControlElement() : true;
        }

        ///
        protected override bool IsEnabledCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsEnabled() : false;
        }

        ///
        protected override bool IsKeyboardFocusableCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsKeyboardFocusable() : false;
        }

        ///
        protected override bool IsOffscreenCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsOffscreen() : true;
        }

        ///
        protected override bool IsPasswordCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsPassword() : false;
        }

        ///
        protected override bool IsRequiredForFormCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.IsRequiredForForm() : false;
        }

        ///
        protected override void SetFocusCore()
        {
            if (this.OwningRowPeer != null)
            {
                this.OwningRowPeer.SetFocus();
            }
        }

        #endregion

        #region IInvokeProvider

        void IInvokeProvider.Invoke()
        {
            EnsureEnabled();

            if (this.OwningRowPeer == null)
            {
                this.OwningDataGrid.ScrollIntoView(_item, null);
            }

            bool success = false;
            if (this.OwningRow != null)
            {
                if (this.OwningDataGrid.EditingRowIndex == this.OwningRow.Index)
                {
                    success = this.OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }
                else
                {
                    this.OwningDataGrid.SetRowSelection(this.OwningRow.Index, true, false);
                    this.OwningDataGrid.SetCurrentCellCore(this.OwningDataGrid.CurrentColumnIndex, this.OwningRow.Index);
                    success = this.OwningDataGrid.BeginEdit();
                }
            }
            if (!success)
            {
                // 

                return;
            }
        }

        #endregion

        #region IScrollItemProvider

        void IScrollItemProvider.ScrollIntoView()
        {
            this.OwningDataGrid.ScrollIntoView(_item, null);
        }

        #endregion

        #region ISelectionItemProvider

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                return this.OwningDataGrid.SelectedItems.Contains(_item);
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                return ProviderFromPeer(_dataGridAutomationPeer);
            }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            EnsureEnabled();

            if (this.OwningDataGrid.SelectionMode == DataGridSelectionMode.Single &&
                this.OwningDataGrid.SelectedItems.Count > 0 &&
                !this.OwningDataGrid.SelectedItems.Contains(_item))
            {
                // 

                return;
            }
            
            int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                this.OwningDataGrid.SetRowSelection(index, true, false);
                return;
            }
            // 

        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            EnsureEnabled();

            int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                bool success = true;
                if (this.OwningDataGrid.EditingRowIndex == index)
                {
                    success = this.OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }
                if (success)
                {
                    this.OwningDataGrid.SetRowSelection(index, false, false);
                    return;
                }
            }
            // 

        }

        void ISelectionItemProvider.Select()
        {
            EnsureEnabled();

            int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                bool success = true;
                if (this.OwningDataGrid.EditingRowIndex != null && this.OwningDataGrid.EditingRowIndex != index)
                {
                    success = this.OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }
                if (success)
                {
                    // Clear all the other selected items and select this one
                    this.OwningDataGrid.ClearRowSelection(index, false);
                    return;
                }
            }
            // 

        }

        #endregion

        #region ISelectionProvider

        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return false;
            }
        }

        bool ISelectionProvider.IsSelectionRequired
        {
            get
            {
                return false;
            }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            if (this.OwningRow != null &&
                this.OwningDataGrid.DisplayData.IsRowDisplayed(this.OwningRow.Index) &&
                this.OwningDataGrid.CurrentRowIndex == this.OwningRow.Index)
            {
                DataGridCell cell = this.OwningRow.Cells[this.OwningRow.OwningGrid.CurrentColumnIndex];
                AutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(cell);
                if (peer != null)
                {
                    return new IRawElementProviderSimple[] { ProviderFromPeer(peer) };
                }
            }
            return null;
        }

        #endregion

        #region Private Methods

        private void EnsureEnabled()
        {
            if (!_dataGridAutomationPeer.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }

        #endregion
    }
}
