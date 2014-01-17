using System.Text;
using System.Xml.Linq;

namespace XmlDiff
{
	public class DiffAttribute : DiffContent
	{
		public DiffAttribute(DiffAction action, XAttribute raw)
		{
			Action = action;
			Raw = raw;
		}

		public DiffAction Action { get; private set; }
		public XAttribute Raw { get; private set; }

		protected internal override void AppendSelfToSb(StringBuilder sb, int level)
		{
			sb.AppendFormat("{0}{1} Attribute: \"{2}\" with value: \"{3}\"\r\n",
				BuildIndent(level), ActionToString(Action), Raw.Name, Raw.Value);
		}
	}
}
