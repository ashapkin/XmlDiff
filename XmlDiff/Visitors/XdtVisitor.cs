using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XmlDiff.Visitors {

	public sealed class XdtContext {
		public XElement XElement { get; private set; }
		public HashSet<XName> SetAttrs { get; private set; }
		public HashSet<XName> RemoveAttrs { get; private set; }
		public bool AttributeChanged { get { return SetAttrs.Any() || RemoveAttrs.Any(); } }
		public bool ValueChanged { get; set; }

		public XdtContext(XElement element) {
			XElement = element;
			SetAttrs = new HashSet<XName>();
			RemoveAttrs = new HashSet<XName>();
		}

		public void HandleAttr(XName attr, DiffAction diffAction) {
			if (diffAction == DiffAction.Added) {
				SetAttrs.Add(attr);
			} else if (diffAction == DiffAction.Removed) {
				RemoveAttrs.Add(attr);
			}
		}
	}

	public class XdtVisitor : IDiffParamsVisitor<XdtContext> {

		public const string XdtNamespaceUri = "http://schemas.microsoft.com/XML-Document-Transform";
		private readonly XDocument _xdt;

		public XdtVisitor() {
			_xdt = new XDocument();
		}

		public XdtContext Initial {
			get { return new XdtContext(new XElement("Initial")); }
		}

		public void Visit(DiffAttribute attr, XdtContext param) {
			param.HandleAttr(attr.Raw.Name, attr.Action);
		}

		public void Visit(DiffValue val, XdtContext param) {
			param.XElement.SetValue(val.Raw);
			param.ValueChanged = true;
		}

		public void Visit(DiffNode node, XdtContext param) {
			if (!node.IsChanged)
				return;

			XElement element = node.DiffAction == DiffAction.Added
				? new XElement(node.Raw)
				: new XElement(node.Raw.Name, node.Raw.Attributes());

			var context = new XdtContext(element);
			if (node.DiffAction == null) {
				foreach (DiffContent item in node.Content) {
					item.Accept(this, context);
				}
			}

			element.SetAttributeValue(XdtElement("Locator"), GetLocator(node, context));
			XElement transformationContext = context.XElement;
			foreach (string transformation in GetElementTransformations(node.DiffAction, context)) {
				transformationContext = GetTransformationContext(transformationContext);
				transformationContext.SetAttributeValue(XdtElement("Transform"), transformation);
				param.XElement.Add(transformationContext);
			}
		}

		private IEnumerable<string> GetElementTransformations(DiffAction? diffAction, XdtContext context) {
			switch (diffAction) {
				case DiffAction.Added:
					yield return "Insert";
					break;
				case DiffAction.Removed:
					yield return "Remove";
					break;
				default:
					if (context.ValueChanged) {
						yield return "Replace";
						//just replace an element, no additional actions to perform
						yield break;
					}
					if (context.AttributeChanged) {
						if (context.SetAttrs.Any()) {
							yield return "SetAttributes(" + string.Join(",", context.SetAttrs) + ")";
						}
						List<XName> uniqueRemoveAttrs = context.RemoveAttrs.Except(context.SetAttrs).ToList();
						if (uniqueRemoveAttrs.Count > 0) {
							yield return "RemoveAttributes(" + string.Join(",", uniqueRemoveAttrs) + ")";
						}
					} else {
						// yield null as a transformation context
						yield return null;
					}
					break;
			}
		}

		private XElement GetTransformationContext(XElement source) {
			//sometimes we need to apply more then a single transformation
			//i.e. add some attribute and remove another at the single time
			//possible solution is to copy current element and specify 
			//an addtional transformation separetely
			if (!source.Attributes(XdtElement("Transform")).Any()) {
				return source;
			}
			return new XElement(source.Name, source.Attributes());
		}

		private string GetLocator(DiffNode node, XdtContext context) {
			// todo: let's use an indexed locator always?
			// pros: less code/logic + it's hard to locate some changes <node attr="123"... to <node attr="newVal"...
			// cons: transformation becomes hard to read?
			if (node.DiffAction == DiffAction.Added) {
				return null;
			}
			XElement rawElement = node.Raw;
			XElement[] prevSiblingsWithSameName = rawElement.ElementsBeforeSelf().Where(e => e.Name == rawElement.Name).ToArray();
			XElement[] nextSiblingsWithSameName = rawElement.ElementsAfterSelf().Where(e => e.Name == rawElement.Name).ToArray();
			XElement[] siblingsWithSameName = prevSiblingsWithSameName.Concat(nextSiblingsWithSameName).ToArray();
			if (siblingsWithSameName.Length == 0) {
				return null;
			}

			XName[] uniqueAttrNames = rawElement.Attributes()
				.Where(a => siblingsWithSameName.All(e => e.Attribute(a.Name)?.Value != a.Value))
				.Select(x => x.Name)
				.Except(context.SetAttrs.Concat(context.RemoveAttrs)).ToArray();
			return uniqueAttrNames.Length == 0
				? "Condition([" + (prevSiblingsWithSameName.Length + 1) + "])"
				: "Match(" + uniqueAttrNames[0] + ")";
		}

		private static XName XdtElement(string name) {
			return XName.Get(name, XdtNamespaceUri);
		}

		// refactor later
		public void Visit(DiffNode node) {
			// TODO: should we reset internal variables / state so that the same visitor instance can be used for multiple diffs?
			XdtContext initial = Initial;
			Visit(node, initial);
			XElement res = initial.XElement.Nodes().OfType<XElement>().FirstOrDefault();
			if (res != null) {
				res.Add(new XAttribute(XNamespace.Xmlns + "xdt", XdtNamespaceUri));
			} else {
				res = new XElement(node.Raw.Name, new XAttribute(XNamespace.Xmlns + "xdt", XdtNamespaceUri));
			}
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

			public override Encoding Encoding {
				get { return Encoding.UTF8; }
			}
		}
	}
}