using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.DisplayHelper.JsonFormatter
{
	/// <summary>
	/// Formats a JSON string with each element on a different line and indented as appropriate to 
	/// show the structure of the data.
	/// </summary>
	/// <remarks>This code copied from http://www.limilabs.com/blog/json-net-formatter
    /// Constructor overload with rootIndentLevel parameter added so can start with an initial 
    /// indent level.  Passes the rootIndentLevel into the new constructor for IndentWriter.
	/// Calling: Pass the unformatted JSON string into the constructor and retrieve the formatted 
	/// text from the Format() method.
	/// </remarks>
	public class JsonFormatter
	{
		StringWalker _walker;
		IndentWriter _writer;
		StringBuilder _currentLine = new StringBuilder();
		bool _quoted;

        public JsonFormatter(string json) : this(json, 0) { }

        public JsonFormatter(string json, int rootIndentLevel)
		{
            _walker = new StringWalker(json);
            _writer = new IndentWriter(rootIndentLevel);
			ResetLine();
		}

		public void ResetLine()
		{
			_currentLine.Length = 0;
		}

		public string Format()
		{
			while (MoveNextChar())
			{
				if (this._quoted == false && this.IsOpenBracket())
				{
					this.WriteCurrentLine();
					this.AddCharToLine();
					this.WriteCurrentLine();
					_writer.Indent();
				}
				else if (this._quoted == false && this.IsCloseBracket())
				{
					this.WriteCurrentLine();
					_writer.UnIndent();
					this.AddCharToLine();
				}
				else if (this._quoted == false && this.IsComma())
				{
					this.AddCharToLine();
					this.WriteCurrentLine();
				}
				else
				{
					AddCharToLine();
				}
			}
			this.WriteCurrentLine();
			return _writer.ToString();
		}

		private bool MoveNextChar()
		{
			bool success = _walker.MoveNext();
			if (this.IsApostrophe())
			{
				this._quoted = !_quoted;
			}
			return success;
		}

		public bool IsApostrophe()
		{
			return this._walker.CharAtIndex() == '"';
		}

		public bool IsOpenBracket()
		{
			return this._walker.CharAtIndex() == '{'
				|| this._walker.CharAtIndex() == '[';
		}

		public bool IsCloseBracket()
		{
			return this._walker.CharAtIndex() == '}'
				|| this._walker.CharAtIndex() == ']';
		}

		public bool IsComma()
		{
			return this._walker.CharAtIndex() == ',';
		}

		private void AddCharToLine()
		{
			this._currentLine.Append(_walker.CharAtIndex());
		}

		private void WriteCurrentLine()
		{
			string line = this._currentLine.ToString().Trim();
			if (line.Length > 0)
			{
				_writer.WriteLine(line);
			}
			this.ResetLine();
		}
	}
}
