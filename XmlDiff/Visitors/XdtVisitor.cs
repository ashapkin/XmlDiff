using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.IO;
//using System.Xml.XPath;

namespace XmlDiff.Visitors
{
    class RemovedAttribute
    {
        public readonly XName Attribute;

        public RemovedAttribute(XName name)
        {
            this.Attribute = name;
        }
    }

    class SetAttribute
    {
        public readonly XName Attribute;

        public SetAttribute(XName name)
        {
            this.Attribute = name;
        }
    }

    class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder builder) : base(builder) { }
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public class XdtVisitor : IDiffVisitor
    {
        private readonly XDocument _xdt;
        private XElement _CurrentElement;
        public const string _XdtNamespaceUri = "http://schemas.microsoft.com/XML-Document-Transform";

        public XdtVisitor()
        {
            this._xdt = new XDocument();
        }

        public string Result
        {
            get
            {

                // https://stackoverflow.com/a/3871822/4473405
                var builder = new StringBuilder();
                using (var writer = new Utf8StringWriter(builder))
                {
                    this._xdt.Save(writer);
                }
                return builder.ToString();
            }
        }

        public void Visit(DiffAttribute attr)
        {
            if (attr.Action == DiffAction.Added)
            {
                this._CurrentElement.Add(attr.Raw);
                this._CurrentElement.AddAnnotation(new SetAttribute(attr.Raw.Name));
            } else
            {
                this._CurrentElement.AddAnnotation(new RemovedAttribute(attr.Raw.Name));
            }
        }

        public void Visit(DiffValue val)
        {
            this._CurrentElement.SetValue(val.Raw);
        }

        public void Visit(DiffNode node)
        {
            // if there are no changes to this element or anything below it, and we have written the root element, then we have nothing to do
            if (!node.IsChanged && this._CurrentElement != null)
                return;

            XElement element = new XElement(node.Raw.Name);
            if (this._CurrentElement == null)
                this._xdt.Add(element);
            else
                this._CurrentElement.Add(element);
            if (this._xdt.Root == element)
            {
                // add the XDT namespace declaration
                element.Add(new XAttribute(XNamespace.Xmlns + "xdt", _XdtNamespaceUri));
            }
            var prev = this._CurrentElement;
            this._CurrentElement = element;
            try
            {
                switch (node.DiffAction)
                {
                    case DiffAction.Removed:
                        element.SetAttributeValue(XName.Get("Transform", _XdtNamespaceUri), "Remove");
                        break;
                    case DiffAction.Added:
                        foreach (var item in node.Raw.Attributes())
                        {
                            this._CurrentElement.Add(item);
                        }
                        this._CurrentElement.SetAttributeValue(XName.Get("Transform", _XdtNamespaceUri), "Insert");
                        foreach (var item in node.Raw.Nodes())
                            this._CurrentElement.Add(item);
                        break;
                    default:
                        foreach (var item in node.Content)
                        {
                            item.Accept(this);
                        }
                        break;
                }
                var set = this._CurrentElement.Annotations(typeof(SetAttribute)).OfType<SetAttribute>().ToArray();
                var remove = this._CurrentElement.Annotations(typeof(RemovedAttribute)).OfType<RemovedAttribute>().Where(a => !set.Any(s => s.Attribute == a.Attribute)).ToArray();

                if (node.DiffAction != DiffAction.Added)
                {
                    // find an attribute whose name and value is the same in both documents
                    var unchanged_attributes = node.Raw.Attributes().Where(a => !set.Any(s => s.Attribute == a.Name) && !remove.Any(s => s.Attribute == a.Name)).ToArray();
                    // restrict that to unique entires
                    var siblings_with_same_name = node.Raw.ElementsBeforeSelf().Concat(node.Raw.ElementsAfterSelf()).Where(e => e.Name == node.Raw.Name).ToArray();
                    var unique_attributes = unchanged_attributes.Where(a => siblings_with_same_name.All(e => e.Attribute(a.Name)?.Value != a.Value));
                    var unique_attribute = unchanged_attributes.FirstOrDefault();
                    if (unique_attribute != null)
                    {
                        this._CurrentElement.Add(unique_attribute);
                        this._CurrentElement.SetAttributeValue(XName.Get("Locator", _XdtNamespaceUri), "Match(" + unique_attribute.Name + ")");
                    }
                    else if (/*unchanged_attributes.Any() &&*/ siblings_with_same_name.Any())
                    {
                        // there is at least one other sibling element with the same name, so locate this element using an XPath Condition of the index
                        this._CurrentElement.SetAttributeValue(XName.Get("Locator", _XdtNamespaceUri), "Condition([" + (node.Raw.NodesBeforeSelf().Count() + 1) + "])");
                    } // otherwise, let the XDT transformer intelligently locate the correct element to transform
                    if (set.Any())
                        this._CurrentElement.SetAttributeValue(XName.Get("Transform", _XdtNamespaceUri), "SetAttributes(" + string.Join(",", set.Select(a => a.Attribute)) + ")");
                    if (remove.Any())
                    {
                        if (set.Any())
                        {
                            var first = this._CurrentElement;
                            // copy this element's name and xdt locator attribute, if present
                            this._CurrentElement = new XElement(first.Name, first.Attribute(XName.Get("Locator", _XdtNamespaceUri)));
                            first.AddAfterSelf(this._CurrentElement);
                        }
                        this._CurrentElement.SetAttributeValue(XName.Get("Transform", _XdtNamespaceUri), "RemoveAttributes(" + string.Join(",", remove.Select(a => a.Attribute)) + ")");
                    }
                }
            }
            finally
            {
                this._CurrentElement = prev;
            }
        }

        public void VisitWithDefaultSettings(DiffNode node)
        {
            // TODO: should we reset internal variables / state so that the same visitor instance can be used for multiple diffs?
            Visit(node);
        }
    }
}
