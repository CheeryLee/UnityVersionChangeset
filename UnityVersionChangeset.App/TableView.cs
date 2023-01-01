using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityVersionChangeset.App
{
    internal class TableView
    {
        internal int TitlePadding { get; set; } = 5;
        internal int DataPadding { get; set; } = 2;
        
        private readonly List<string> _headers;
        private readonly List<List<string>> _data;
        private readonly StringBuilder _builder;

        internal TableView(List<string> headers, List<List<string>> data)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers), "Headers can't be null");

            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can't be null");

            if (data.Count > 0 && headers.Count != data[0].Count)
                throw new ArgumentException("Data columns count must be equal to header elements count");

            _headers = headers;
            _data = data;
            _builder = new StringBuilder();
        }

        internal void Print()
        {
            CreateHeader();
            CreateData();
        }

        private void CreateHeader()
        {
            _builder.Clear();
            var width = _headers.Select(x => x.Length).Sum() + _headers.Count * TitlePadding * 2 + (_headers.Count - 1);
            
            _builder.Append('-', width);
            var rowStr = _builder.ToString();
            
            _builder.Clear();

            for (var i = 0; i < _headers.Count; i++)
            {
                _builder.Append(' ', TitlePadding);
                _builder.Append(_headers[i]);
                _builder.Append(' ', TitlePadding);
                
                if (i < _headers.Count - 1)
                    _builder.Append('|');
            }

            var titleStr = _builder.ToString();
            
            Console.WriteLine(rowStr);
            
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(titleStr);
            Console.ResetColor();
            
            Console.WriteLine(rowStr);
        }

        private void CreateData()
        {
            for (var i = 0; i < _data.Count; i++)
            {
                _builder.Clear();
                var row = _data[i];

                for (var j = 0; j < row.Count && j < _headers.Count; j++)
                {
                    var data = row[j];
                    var width = TitlePadding * 2 + _headers[j].Length;

                    if (data.Length > width - DataPadding * 2)
                        data = data[..(width - DataPadding * 2 - 3)] + "...";

                    _builder.Append(' ', 2);
                    _builder.Append(data);
                    _builder.Append(' ', width - data.Length - DataPadding + 1);
                }
                
                Console.WriteLine(_builder);
            }
        }
    }
}