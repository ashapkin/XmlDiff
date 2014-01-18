using System;
using XmlDiff.Visitors;

namespace XmlDiff
{
	public class DiffValue : DiffContent
	{
		public DiffValue(DiffAction action, string raw)
		{
			if (string.IsNullOrEmpty(raw))
				throw new ArgumentNullException("raw");

			Action = action;
			Raw = raw;
		}

		public DiffAction Action { get; private set; }
		public string Raw { get; private set; }

		public override void Accept(IDiffVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void Accept<T>(IDiffParamsVisitor<T> visitor, T param)
		{
			visitor.Visit(this, param);
		}

		public override string ToString()
		{
			var visitor = new ToStringVisitor();
			visitor.Visit(this, 0);
			return visitor.Result;
		}
	}
}
