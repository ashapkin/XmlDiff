using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace XmlDiff.Visitors
{
	public class HtmlVisitor : IDiffParamsVisitor<int>
	{
		protected readonly int MaxAttributesPreviewCount = 2;
		protected readonly string Styles =
			string.Join(string.Empty,
			"<style type=\"text/css\">",
			"span { margin:5px;}",
			".removed { background-color : #ffe6e6; }",
			".added { background-color : #e6ffe6; }",
			"</style>");

		private readonly StringBuilder _sb = new StringBuilder();

		public HtmlVisitor()
		{
			_sb.Append(Styles);
		}

		public string Result
		{
			get { return _sb.ToString(); }
		}

		public void Visit(DiffAttribute attr, int level)
		{
			_sb.AppendFormat("<span{0}>\"{1}\"=\"{2}\"</span>",
				ActionToString(attr.Action), attr.Raw.Name, attr.Raw.Value);
		}

		public void Visit(DiffValue val, int level)
		{
			string indent = BuildIndent(level);
			_sb.AppendFormat("{0}<span{0}>{1}</span>", indent, ActionToString(val.Action), val.Raw);
		}

		public void Visit(DiffNode node, int level)
		{
			if (level > 0)
			{
				_sb.Append("</div>");
			}

			string indent = BuildIndent(level);
			string action = ActionToString(node.DiffAction);
			_sb.Append("<div>");
			_sb.AppendFormat("{0}<span{1}>&lt{2}", indent, action, node.Raw.Name);
			if (node.DiffAction == null)
			{
				foreach (DiffContent content in node.Content)
				{
					content.Accept(this, level + 1);
				}
			}
			else
			{
				AppendRawAttributesToSb(node, _sb);
			}

			if (level == 0)
			{
				_sb.Append("</div>");
			}
		}

		private void AppendRawAttributesToSb(DiffNode node, StringBuilder sb)
		{
			if (node.Raw.HasAttributes)
			{
				IEnumerator<XAttribute> enumerator = node.Raw.Attributes().GetEnumerator();
				int left = MaxAttributesPreviewCount;
				while (enumerator.MoveNext() && left > 0)
				{
					XAttribute attr = enumerator.Current;
					sb.AppendFormat("<span>\"{0}\"=\"{1}\"</span>", attr.Name, attr.Value);
					left--;
				}
				if (enumerator.MoveNext())
				{
					sb.Append("<span>...</span>");
				}
			}
			else
			{
				sb.Append("<span>...</span>");
			}
		}

		private static string BuildIndent(int level)
		{
			var sb = new StringBuilder();
			while (level > 0)
			{
				sb.Append("<span class=\"indent\">&rarr;</span>");
				level--;
			}
			return sb.ToString();
		}

		private static string ActionToString(DiffAction? diffAction)
		{
			if (diffAction == DiffAction.Added)
			{
				return " class=\"added\"";
			}
			if (diffAction == DiffAction.Removed)
			{
				return " class=\"removed\"";
			}
			return string.Empty;
		}
	}
}
