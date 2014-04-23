using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.DisplayHelper.JsonFormatter
{
	/// <summary>
	/// For stepping through a string, one character at a time.
	/// </summary>
	/// <remarks>This code copied verbatim from http://www.limilabs.com/blog/json-net-formatter
	/// </remarks>
	public class StringWalker
	{
		string _s;
		int _index = 0;
		public int Index 
		{
			get { return _index;  }
			set { _index = value; }
		}

		public StringWalker(string s)
		{
			_s = s;
			Index = -1;
		}

		public bool MoveNext()
		{
			if (Index == _s.Length - 1)
				return false;
			Index++;
			return true;
		}

		public char CharAtIndex()
		{
			return _s[Index];
		}
	}
}
