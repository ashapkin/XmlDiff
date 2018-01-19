using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace XmlDiff.Visitors
{
	public class HtmlVisitor : IDiffParamsVisitor<int>
	{
		protected int MaxAttributesPreviewCount { get { return 2; } }
		protected virtual string Styles
		{
			get
			{
				return
					string.Join(Environment.NewLine,
					"<body><style type=\"text/css\">",
					"span { margin:5px;}",
					".removed { background-color : #ffe6e6; text-decoration:line-through; }",
					".added { background-color : #e6ffe6; }",
					"</style>");
			}
		}

		private readonly StringBuilder _sb = new StringBuilder();
		private uint _lineNumber;
		private bool _closeLineWithoutPrefixMode;

		public HtmlVisitor()
		{
			_sb.Append(Styles);
		}

		public string Result
		{
			get { return _sb.ToString(); }
		}

		public void Visit(DiffAttribute attr, int param)
		{
			_sb.AppendFormat("<span{0}>\"{1}\"=\"{2}\"</span>",
				ActionToString(attr.Action), attr.Raw.Name, System.Security.SecurityElement.Escape(attr.Raw.Value));
		}

		public void Visit(DiffValue val, int param)
		{
			string indent = string.Empty;
			if (!_closeLineWithoutPrefixMode)
			{
				DrawLineBreak();
				indent = BuildIndent(param);
				_closeLineWithoutPrefixMode = true;
			}
			_sb.AppendFormat("{0}<span{1}>{2}</span>", indent, ActionToString(val.Action), System.Security.SecurityElement.Escape(val.Raw));
		}

		public void Visit(DiffNode node, int param)
		{
			DrawLineBreak();

			string indent = BuildIndent(param);
			string action = ActionToString(node.DiffAction);
			_sb.AppendFormat("{0}<span{1}>&lt;{2}", indent, action, node.Raw.Name);

			if (node.DiffAction == null)
			{
				_sb.Append("</span>");
				foreach (DiffContent content in node.Content)
				{
					content.Accept(this, param + 1);
				}
				DrawLineBreak();

				string closingTag = string.Format("{0}<span{1}>&lt;/{2}&gt;</span>", indent, action, node.Raw.Name);
				DrawLineBreak(openNew: param != 0, closingPrefix: closingTag);
				_closeLineWithoutPrefixMode = true;

			} else
			{
				AppendRawAttributesToSb(node, _sb);
				_sb.Append("/");
				_sb.Append("</span>");
			}
		}

		public void VisitWithDefaultSettings(DiffNode node)
		{
			Visit(node, 0);
			_sb.Append("</body>");
		}

		private void DrawLineBreak(bool openNew = true, string closingPrefix = "&gt;")
		{
			if (_lineNumber > 0)
			{
				if (!_closeLineWithoutPrefixMode)
				{
					_sb.Append(closingPrefix);
				}
				_sb.Append("</div>");
			}
			if (openNew)
			{
				_sb.Append("<div>");
				_lineNumber++;
			}

			_sb.Append(Environment.NewLine);
			_closeLineWithoutPrefixMode = false;
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
					sb.AppendFormat("<span>\"{0}\"=\"{1}\"</span>", attr.Name, System.Security.SecurityElement.Escape(attr.Value));
					left--;
				}
				if (enumerator.MoveNext())
				{
					sb.Append("<span>...</span>");
				}
			} else
			{
				//sb.Append("<span>...</span>");
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
