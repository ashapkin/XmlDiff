using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XmlDiff.Visitors;

namespace XmlDiff
{
	public class DiffNode : DiffContent
	{
		public DiffNode(DiffAction action, XElement raw)
			: this(raw, null)
		{
			DiffAction = action;
		}

		public DiffNode(XElement raw, IEnumerable<DiffContent> content)
		{
			Raw = raw ?? throw new ArgumentNullException(nameof(raw));
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

		//just for easier testing
		internal IEnumerable<DiffAttribute> Attributes { get { return Content.OfType<DiffAttribute>(); } }
		internal IEnumerable<DiffNode> Childs { get { return Content.OfType<DiffNode>(); } }
		internal IEnumerable<DiffValue> Values { get { return Content.OfType<DiffValue>(); } }
	}
}
