﻿using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XmlDiff.Visitors
{
	public class ToStringVisitor : IDiffParamsVisitor<int>
	{
		private static readonly int MaxAttributesPreviewCount = 2;
		private readonly StringBuilder _sb = new StringBuilder();

		public string Result
		{
			get { return _sb.ToString(); }
		}

		public void Visit(DiffAttribute attr, int param)
		{
			int level = param;
			_sb.AppendFormat("{0}{1} Attribute: \"{2}\" with value: \"{3}\"\r\n",
				BuildIndent(level), ActionToString(attr.Action), attr.Raw.Name, attr.Raw.Value);
		}

		public void Visit(DiffValue val, int param)
		{
			int level = param;
			string indent = BuildIndent(level);
			_sb.AppendFormat("{0}{1} Value: \"{2}\"\r\n", indent, ActionToString(val.Action), val.Raw);
		}

		public void Visit(DiffNode node, int param)
		{
			int level = param;
			if (node.IsChanged)
			{
				string indent = BuildIndent(level);
				string action = ActionToString(node.DiffAction);
				_sb.AppendFormat("{0}{1} Element \"{2}\"", indent, action, node.Raw.Name);
				AppendRawAttributesToSb(node, _sb);
				_sb.Append("\r\n");
				if (node.DiffAction == null)
				{
					foreach (DiffContent content in node.Content)
					{
						content.Accept(this, level + 1);
					}
				}
			}
		}

		private static void AppendRawAttributesToSb(DiffNode node, StringBuilder sb)
		{
			if (node.Raw.HasAttributes)
			{
				foreach (XAttribute attr in node.Raw.Attributes().Take(MaxAttributesPreviewCount))
				{
					sb.AppendFormat(" \"{0}\"=\"{1}\"", attr.Name, attr.Value);
				}
			}
		}

		private static string BuildIndent(int level)
		{
			var sb = new StringBuilder();
			while (level > 0)
			{
				sb.Append("...");
				level--;
			}
			return sb.ToString();
		}

		private static string ActionToString(DiffAction? diffAction)
		{
			if (diffAction == DiffAction.Added)
			{
				return "+";
			}
			if (diffAction == DiffAction.Removed)
			{
				return "-";
			}

			return "=";
		}
	}
}
