using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XmlDiff
{
	public class DiffNode : DiffContent
	{
		private readonly static int MaxAttributesPreviewCount = 2;

		public DiffNode(DiffAction action, XElement raw)
			: this(raw, null)
		{
			DiffAction = action;
		}

		public DiffNode(XElement raw, IEnumerable<DiffContent> content)
		{
			if (raw == null)
				throw new ArgumentNullException("raw");

			Raw = raw;
			Content = content ?? Enumerable.Empty<DiffContent>();
		}

		public DiffAction? DiffAction { get; private set; }
		public XElement Raw { get; private set; }
		public IEnumerable<DiffContent> Content { get; private set; }
		private bool? isChanged;
		public override bool IsChanged
		{
			get
			{
				if (isChanged == null)
				{
					isChanged = DiffAction != null || Content.Any(x => x.IsChanged);
				}
				return isChanged.Value;
			}
		}

		protected internal override void AppendSelfToSb(StringBuilder sb, int level)
		{
			if (IsChanged)
			{
				sb.AppendFormat("{0}{1} Element \"{2}\"", BuildIndent(level), ActionToString(DiffAction), Raw.Name);
				AppendRawAttributesToSb(sb);
				sb.Append("\r\n");
				if (DiffAction == null)
				{
					foreach (DiffContent content in Content)
					{
						content.AppendSelfToSb(sb, level + 1);
					}
				}
			}
		}

		//to make a distinguish between even tags
		private void AppendRawAttributesToSb(StringBuilder sb)
		{
			if (Raw.HasAttributes)
			{
				foreach (XAttribute attr in Raw.Attributes().Take(MaxAttributesPreviewCount))
				{
					sb.AppendFormat(" \"{0}\"=\"{1}\"", attr.Name, attr.Value);
				}
			}
		}

		//just for easier testing
		internal IEnumerable<DiffAttribute> Attributes { get { return Content.OfType<DiffAttribute>(); } }
		internal IEnumerable<DiffNode> Childs { get { return Content.OfType<DiffNode>(); } }
		internal IEnumerable<DiffValue> Values { get { return Content.OfType<DiffValue>(); } }
	}
}
