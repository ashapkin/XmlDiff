using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using XmlDiff.Visitors;

namespace XmlDiff.Tests
{
    [TestFixture]
    class XdtVisitorTests
    {
        private XmlComparer _xmlDiff = new XmlComparer();
        private XDocument _simpleDoc = new XDocument(new XElement("configuration",
                new XElement("appSettings",
                    new XElement("add", new XAttribute("key", "example"), new XAttribute("value", "foobar")),
                    new XElement("add", new XAttribute("key", "hello"), new XAttribute("value", "world"))
                )
            ));

        [Test]
        public void Xdt_SameDocument_ProducesBlankXdt()
        {
            DiffNode output = _xmlDiff.Compare(_simpleDoc.Root, new XDocument(_simpleDoc).Root);

            var visitor = new XdtVisitor();
            visitor.VisitWithDefaultSettings(output);
            var result = XDocument.Parse(visitor.Result);

            // document should have a root element with the same name as in the original document
            Assert.AreEqual(result.Root.Name, _simpleDoc.Root.Name);
            // that root element should have the xdt prefix namespace
            Assert.AreEqual(result.Root.GetPrefixOfNamespace(XdtVisitor._XdtNamespaceUri), "xdt");
            // that root element should be empty, as there are no differences
            Assert.IsFalse(result.Root.HasElements);
        }

        [Test]
        public void Xdt_ElementsHaveLocatorAndTransformWhenNeeded()
        {
            var destDoc = new XDocument(_simpleDoc);
            destDoc.Root.Element("appSettings").Elements().First().SetAttributeValue("value", "test");
            destDoc.Root.Element("appSettings").Add(new XElement("add", new XAttribute("key", "foo"), new XAttribute("value", "bar")));

            DiffNode output = _xmlDiff.Compare(_simpleDoc.Root, destDoc.Root);

            var visitor = new XdtVisitor();
            visitor.VisitWithDefaultSettings(output);
            var result = XDocument.Parse(visitor.Result);

            // root element should have no locator or transformation
            Assert.IsNull(_FindXdtAttribute(result.Root));
            // same for appSettings
            Assert.IsNull(_FindXdtAttribute(result.Root.Element("appSettings")));
            // there should be two children - one updated, one inserted - the one that stays the same shouldn't appear in the transformation file
            var children = result.Root.Element("appSettings").Elements();
            Assert.AreEqual(children.Count(), 2);
            // there should be a Locator and a Transform on the changed setting
            Assert.AreEqual(_FindXdtAttribute(children.First(), "Locator").Value, "Match(key)");
            Assert.AreEqual(_FindXdtAttribute(children.First(), "Transform").Value, "SetAttributes(value)");
            // there should be a Transform on the new setting
            Assert.AreEqual(_FindXdtAttribute(children.Last(), "Transform").Value, "Insert");
            Assert.IsNull(_FindXdtAttribute(children.Last(), "Locator"));
        }

        [Test]
        public void Xdt_TestRemoveUniqueElement()
        {
            var destDoc = new XDocument(_simpleDoc);
            destDoc.Root.Element("appSettings").Remove();
            
            DiffNode output = _xmlDiff.Compare(_simpleDoc.Root, destDoc.Root);

            var visitor = new XdtVisitor();
            visitor.VisitWithDefaultSettings(output);
            var result = XDocument.Parse(visitor.Result);

            // root element should have no locator or transformation
            Assert.IsNull(_FindXdtAttribute(result.Root));
            // removed element should have a Transform but as it is unique, no Locator
            Assert.AreEqual(_FindXdtAttribute(result.Root.Element("appSettings"), "Transform").Value, "Remove");
            Assert.IsNull(_FindXdtAttribute(result.Root.Element("appSettings"), "Locator"));
        }

        [Test]
        public void Xdt_TestNonUniqueElementNoUnchangedAttribute()
        {
            var sourceDoc = new XDocument(_simpleDoc);
            sourceDoc.Root.Element("appSettings").AddFirst(new XElement("test"));
            var destDoc = new XDocument(sourceDoc);
            destDoc.Root.Element("appSettings").Elements("add").First().SetAttributeValue("value", "test value");
            destDoc.Root.Element("appSettings").Elements("add").First().SetAttributeValue("key", "test_key");

            DiffNode output = _xmlDiff.Compare(sourceDoc.Root, destDoc.Root);

            var visitor = new XdtVisitor();
            visitor.VisitWithDefaultSettings(output);
            var result = XDocument.Parse(visitor.Result);

            // root element should have no locator or transformation
            Assert.IsNull(_FindXdtAttribute(result.Root));
            // changed element should be matched by a condition
            Assert.AreEqual(_FindXdtAttribute(result.Root.Element("appSettings").Elements("add").First(), "Transform").Value, "SetAttributes(key,value)");
            Assert.AreEqual(_FindXdtAttribute(result.Root.Element("appSettings").Elements("add").First(), "Locator").Value, "Condition([1])");
        }

        [Test]
        public void Xdt_TestRemoveAndSetAttribute()
        {
            var destDoc = new XDocument(_simpleDoc);
            destDoc.Root.Element("appSettings").Elements().First().Attributes("value").Remove();
            destDoc.Root.Element("appSettings").Elements().First().SetAttributeValue("key", "test_key");

            DiffNode output = _xmlDiff.Compare(_simpleDoc.Root, destDoc.Root);

            var visitor = new XdtVisitor();
            visitor.VisitWithDefaultSettings(output);
            var result = XDocument.Parse(visitor.Result);

            // set and remove isn't possible in one go, so should be two instances with the same locator
            Assert.AreEqual(_FindXdtAttribute(result.Root.Element("appSettings").Elements().First(), "Transform").Value, "SetAttributes(key)");
            var second = result.Root.Element("appSettings").Elements().Skip(1).First();
            Assert.AreEqual(result.Root.Element("appSettings").Elements().First().Name, second.Name);
            Assert.AreEqual(_FindXdtAttribute(second, "Locator").Value, _FindXdtAttribute(result.Root.Element("appSettings").Elements().First(), "Locator").Value);
            Assert.AreEqual(_FindXdtAttribute(second, "Transform").Value, "RemoveAttributes(value)");
        }

        private XAttribute _FindXdtAttribute(XElement element, string name = null)
        {
            var attrs = element.Attributes().Where(a => !a.IsNamespaceDeclaration).Where(a => a.Name.NamespaceName == XdtVisitor._XdtNamespaceUri);
            return attrs.FirstOrDefault(a => a.Name.LocalName == name || string.IsNullOrEmpty(name));
        }
    }
}
