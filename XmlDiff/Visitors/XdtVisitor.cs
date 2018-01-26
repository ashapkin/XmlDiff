using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XmlDiff.Visitors {

	public sealed class XdtContext {

		private readonly HashSet<XName> _removedAttres = new HashSet<XName>();
		private readonly HashSet<XName> _addedAttrs = new HashSet<XName>();

		public XElement CurrentElement { get; private set; }

		public IEnumerable<XName> RemoveAttrs {
			get {
				var remaining = new HashSet<XName>(_removedAttres);
				remaining.ExceptWith(_addedAttrs);
				return remaining;
			}
		}

		public IEnumerable<XName> SetAttrs {
			get { return _addedAttrs; }
		}

		public IEnumerable<XName> Affected {
			get { return _addedAttrs.Concat(_removedAttres); }
		}

		public XdtContext(XElement element) {
			CurrentElement = element;
		}

		public void HandleAttr(XName attr, DiffAction diffAction) {
			if (diffAction == DiffAction.Added) {
				_addedAttrs.Add(attr);
			}
			if (!_addedAttrs.Contains(attr)) {
				_removedAttres.Add(attr);
			}
		}
	}

	public class XdtVisitor : IDiffParamsVisitor<XdtContext> {

		public const string _XdtNamespaceUri = "http://schemas.microsoft.com/XML-Document-Transform";
		private readonly XDocument _xdt;

		public XdtVisitor() {
			_xdt = new XDocument();
		}

		public void Visit(DiffAttribute attr, XdtContext param) {
			param.HandleAttr(attr.Raw.Name, attr.Action);
		}

		public void Visit(DiffValue val, XdtContext param) {
			param.CurrentElement.SetValue(val.Raw);
			param.CurrentElement.SetAttributeValue(XdtElement("Transform"), "Replace");
		}

		public void Visit(DiffNode node, XdtContext param) {
			if (!node.IsChanged)
				return;

			XdtContext context = YieldCurrentContext(node);
			param.CurrentElement.Add(context.CurrentElement);

			if (node.DiffAction != DiffAction.Added) {
				string locator = GetLocator(node, context.Affected);
				if (locator != null) {
					context.CurrentElement.SetAttributeValue(XdtElement("Locator"), locator);
				}

				if (context.RemoveAttrs.Any()) {
					context.CurrentElement.SetAttributeValue(XdtElement("Transform"), "RemoveAttributes(" + string.Join(",", context.RemoveAttrs) + ")");
				}
				if (context.SetAttrs.Any()) {
					GetNodeToAddAttributes(context).SetAttributeValue(XdtElement("Transform"), "SetAttributes(" + string.Join(",", context.SetAttrs) + ")");
				}
			}
		}

		private XElement GetNodeToAddAttributes(XdtContext newContent) {
			if (newContent.RemoveAttrs.Any()) {
				var copy = new XElement(newContent.CurrentElement.Name, newContent.CurrentElement.Attributes());
				newContent.CurrentElement.AddAfterSelf(copy);
				return copy;
			}
			return newContent.CurrentElement;
		}

		private string GetLocator(DiffNode node, IEnumerable<XName> affectedAttrs) {
			// todo: let's use an indexed locator always?
			// pros: less code/logic + it's hard to locate some changes <node attr="123"... to <node attr="newVal"...
			// cons: transformation becomes hard to read?
			XElement[] prevSiblingsWithSameName = node.Raw.ElementsBeforeSelf().Where(e => e.Name == node.Raw.Name).ToArray();
			XElement[] nextSiblingsWithSameName = node.Raw.ElementsAfterSelf().Where(e => e.Name == node.Raw.Name).ToArray();
			XElement[] siblingsWithSameName = prevSiblingsWithSameName.Concat(nextSiblingsWithSameName).ToArray();
			if (siblingsWithSameName.Length == 0) {
				return null;
			}

			XName[] uniqueAttrNames = node.Raw.Attributes()
				.Where(a => siblingsWithSameName.All(e => e.Attribute(a.Name)?.Value != a.Value))
				.Select(x => x.Name)
				.Except(affectedAttrs).ToArray();
			return uniqueAttrNames.Length == 0
					? "Condition([" + (prevSiblingsWithSameName.Length + 1) + "])"
					: "Match(" + uniqueAttrNames[0] + ")";
		}

		private XdtContext YieldCurrentContext(DiffNode node) {
			if (node.DiffAction == DiffAction.Added) {
				var elem = new XElement(node.Raw);
				elem.SetAttributeValue(XdtElement("Transform"), "Insert");
				return new XdtContext(elem);
			}

			var newElement = new XElement(node.Raw.Name, node.Raw.Attributes());
			var newContext = new XdtContext(newElement);
			if (node.DiffAction == DiffAction.Removed) {
				newElement.SetAttributeValue(XdtElement("Transform"), "Remove");
			} else {
				foreach (DiffContent item in node.Content) {
					item.Accept(this, newContext);
				}
			}
			return newContext;
		}

		private static XName XdtElement(string name) {
			return XName.Get(name, _XdtNamespaceUri);
		}

		// refactor later
		public void VisitWithDefaultSettings(DiffNode node) {
			// TODO: should we reset internal variables / state so that the same visitor instance can be used for multiple diffs?
			// I think we need change interface
			var initial = new XdtContext(new XElement("Temp"));
			Visit(node, initial);
			XElement res = initial.CurrentElement.Nodes().OfType<XElement>().First();
			res.Add(new XAttribute(XNamespace.Xmlns + "xdt", _XdtNamespaceUri));
			_xdt.Add(res);
		}

		public string Result {
			get {
				// https://stackoverflow.com/a/3871822/4473405
				var builder = new StringBuilder();
				using (var writer = new Utf8StringWriter(builder)) {
					_xdt.Save(writer);
				}
				return builder.ToString();
			}
		}

		private sealed class Utf8StringWriter : StringWriter {
			public Utf8StringWriter(StringBuilder builder) : base(builder) { }
			public override Encoding Encoding { get { return Encoding.UTF8; } }
		}
	}
}
