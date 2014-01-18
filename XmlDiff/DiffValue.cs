using System;
using System.Text;

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

		protected internal override void AppendSelfToSb(StringBuilder sb, int level)
		{
			sb.AppendFormat("{0}{1} Value: \"{2}\"\r\n", BuildIndent(level), ActionToString(Action), Raw);
		}
	}
}
