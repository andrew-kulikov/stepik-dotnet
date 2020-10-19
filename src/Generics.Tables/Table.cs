using System;
using System.Collections.Generic;
using System.Linq;

namespace Generics.Tables
{
    public class Table<TRow, TColumn, TValue>
    {
        public List<TRow> Rows { get; } = new List<TRow>();
        public List<TColumn> Columns { get; } = new List<TColumn>();

        private readonly Dictionary<TRow, Dictionary<TColumn, TValue>> _values =
            new Dictionary<TRow, Dictionary<TColumn, TValue>>();

        public void AddColumn(TColumn column)
        {
            if (!Columns.Any(r => r.Equals(column))) Columns.Add(column);
        }

        public void AddRow(TRow row)
        {
            if (!Rows.Any(r => r.Equals(row))) Rows.Add(row);
        }

        public Table()
        {
            Open = new OpenIndexer(this);
            Existed = new ExistedIndexer(this);
        }

        public OpenIndexer Open { get; }
        public ExistedIndexer Existed { get; }

        public class OpenIndexer
        {
            private readonly Table<TRow, TColumn, TValue> _table;

            public OpenIndexer(Table<TRow, TColumn, TValue> table)
            {
                _table = table;
            }

            public TValue this[TRow row, TColumn column]
            {
                get
                {
                    if (!_table._values.ContainsKey(row) || !_table._values[row].ContainsKey(column))
                    {
                        return default(TValue);
                    }

                    return _table._values[row][column];
                }
                set
                {
                    CreateIndex(row, column);

                    _table._values[row][column] = value;
                }
            }

            private void CreateIndex(TRow row, TColumn column)
            {
                _table.AddRow(row);
                _table.AddColumn(column);

                if (!_table._values.ContainsKey(row))
                {
                    _table._values[row] = new Dictionary<TColumn, TValue>();
                }
            }
        }

        public class ExistedIndexer
        {
            private readonly Table<TRow, TColumn, TValue> _table;

            public ExistedIndexer(Table<TRow, TColumn, TValue> table)
            {
                _table = table;
            }

            public TValue this[TRow row, TColumn column]
            {
                get
                {
                    AssertIndexExist(row, column);

                    if (!_table._values.ContainsKey(row) || !_table._values[row].ContainsKey(column))
                    {
                        return default(TValue);
                    }

                    return _table._values[row][column];
                }
                set
                {
                    AssertIndexExist(row, column);

                    if (!_table._values.ContainsKey(row))
                    {
                        _table._values[row] = new Dictionary<TColumn, TValue>();
                    }

                    _table._values[row][column] = value;
                }
            }

            private void AssertIndexExist(TRow row, TColumn column)
            {
                if (!_table.Rows.Any(r => r.Equals(row)) || !_table.Columns.Any(r => r.Equals(column)))
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}