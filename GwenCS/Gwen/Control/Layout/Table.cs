﻿using System;
using System.Linq;

namespace Gwen.Control.Layout
{
    /// <summary>
    /// Base class for multi-column tables.
    /// </summary>
    public class Table : Base
    {
        // only children of this control should be TableRow.

        private bool m_SizeToContents;
        private int m_ColumnCount;
        private readonly int m_DefaultRowHeight;

        private readonly int[] m_ColumnWidth;

        /// <summary>
        /// Column count (default 1).
        /// </summary>
        public int ColumnCount { get { return m_ColumnCount; } set { SetColumnCount(value); } }

        /// <summary>
        /// Row count.
        /// </summary>
        public int RowCount { get { return Children.Count; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Table(Base parent) : base(parent)
        {
            m_ColumnCount = 1;
            m_DefaultRowHeight = 22;

            m_ColumnWidth = new int[TableRow.MaxColumns];

            for (int i = 0; i < TableRow.MaxColumns; i++)
            {
                m_ColumnWidth[i] = 20;
            }

            m_SizeToContents = false;
        }

        /// <summary>
        /// Sets the number of columns.
        /// </summary>
        /// <param name="count">Number of columns.</param>
        public void SetColumnCount(int count)
        {
            if (m_ColumnCount == count) return;
            foreach (TableRow row in Children.OfType<TableRow>())
            {
                row.ColumnCount = count;
            }

            m_ColumnCount = count;
        }

        /// <summary>
        /// Sets the column width (in pixels).
        /// </summary>
        /// <param name="column">Column index.</param>
        /// <param name="width">Column width.</param>
        public void SetColumnWidth(int column, int width)
        {
            if (m_ColumnWidth[column] == width) return;
            m_ColumnWidth[column] = width;
            Invalidate();
        }

        /// <summary>
        /// Gets the column width (in pixels).
        /// </summary>
        /// <param name="column">Column index.</param>
        /// <returns>Column width.</returns>
        public int SetColumnWidth(int column)
        {
            return m_ColumnWidth[column];
        }

        /// <summary>
        /// Adds a new empty row.
        /// </summary>
        /// <returns>Newly created row.</returns>
        public TableRow AddRow()
        {
            TableRow row = new TableRow(this);
            row.ColumnCount = m_ColumnCount;
            row.Height = m_DefaultRowHeight;
            row.Dock = Pos.Top;
            return row;
        }

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="row">Row to add.</param>
        public void AddRow(TableRow row)
        {
            row.Parent = this;
            row.ColumnCount = m_ColumnCount;
            row.Height = m_DefaultRowHeight;
            row.Dock = Pos.Top;
        }

        /// <summary>
        /// Removes a row by reference.
        /// </summary>
        /// <param name="row">Row to remove.</param>
        public void RemoveRow(TableRow row)
        {
            RemoveChild(row, true);
        }

        /// <summary>
        /// Removes a row by index.
        /// </summary>
        /// <param name="idx">Row index.</param>
        public void RemoveRow(int idx)
        {
            var row = Children[idx];
            RemoveRow(row as TableRow);
        }

        /// <summary>
        /// Removes all rows.
        /// </summary>
        public void RemoveAll()
        {
            foreach (TableRow child in Children) // all should be of type TableRow
            {
                RemoveRow(child);
            }
        }

        /// <summary>
        /// Gets the index of a specified row.
        /// </summary>
        /// <param name="row">Row to search for.</param>
        /// <returns>Row index if found, -1 otherwise.</returns>
        public int GetRowIndex(TableRow row)
        {
            return Children.IndexOf(row);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Layout(Skin.Base skin)
        {
            base.Layout(skin);
         
            if (m_SizeToContents)
            {
                DoSizeToContents();
            }

            bool even = false;
            foreach (TableRow row in Children)
            {
                row.EvenRow = even;
                even = !even;
                for (int i = 0; i < TableRow.MaxColumns && i < m_ColumnCount; i++)
                {
                    row.SetColumnWidth(i, m_ColumnWidth[i]);
                }
            }
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void PostLayout(Skin.Base skin)
        {
            if (m_SizeToContents)
            {
                SizeToChildren();
                m_SizeToContents = false;
            }
        }

        /// <summary>
        /// Sizes to fit contents.
        /// </summary>
        public void SizeToContents()
        {
            m_SizeToContents = true;
            Invalidate();
        }

        protected void DoSizeToContents()
        {
            for (int i = 0; i < TableRow.MaxColumns; i++)
            {
                m_ColumnWidth[i] = 10;
            }

            foreach (TableRow row in Children)
            {
                row.SizeToContents();

                for (int i = 0; i < TableRow.MaxColumns; i++)
                {
                    if (null != row.GetColumn(i))
                    {
                        m_ColumnWidth[i] = Math.Max(m_ColumnWidth[i], row.GetColumn(i).Width);
                    }
                }
                //iBottom += pRow->Height();
            }

            InvalidateParent();
        }
    }
}
