using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.DisplayHelper.JsonFormatter
{
	/// <summary>
	/// For building a multi-line string, with each line indented by an arbitrary amount.
	/// </summary>
	/// <remarks>This code copied from http://www.limilabs.com/blog/json-net-formatter
	/// _tabWidth added to allow the indent width to be easily changed, CreateIndent rewritten to 
	/// simplify it.  Constructor and _rootIndentLevel added so can start with an initial indent 
    /// level.
	/// </remarks>
	public class IndentWriter
	{
		StringBuilder _sb = new StringBuilder();
		int _indentLevel;
        int _rootIndentLevel;
		int _tabWidth = 4;

        public IndentWriter(int rootIndentLevel)
        {
            _rootIndentLevel = rootIndentLevel;
            _indentLevel = rootIndentLevel;
        }

		public void Indent()
		{
			_indentLevel++;
		}

		public void UnIndent()
		{
			if (_indentLevel > _rootIndentLevel)
				_indentLevel--;
		}

		public void WriteLine(string line)
		{
			_sb.AppendLine(CreateIndent() + line);
		}

		private string CreateIndent()
		{
			return new string(' ', _tabWidth * _indentLevel);
		}

		public override string ToString()
		{
			return _sb.ToString();
		}
	}
}
