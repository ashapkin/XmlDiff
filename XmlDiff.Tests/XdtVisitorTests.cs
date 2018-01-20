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

        [Test]
        public void Compare_SameDocument()
        {
            var sourceDoc = new XDocument(new XElement("configuration", 
                new XElement("appSettings",
                    new XElement("add", new XAttribute("key", "example"), new XAttribute("value", "foobar"))
                )
            ));
            
            DiffNode output = _xmlDiff.Compare(sourceDoc.Root, new XDocument(sourceDoc).Root);

            var visitor = new XdtVisitor();
            visitor.VisitWithDefaultSettings(output);
            var result = XDocument.Parse(visitor.Result);

            // document should have a root element with the same name as in the original document
            Assert.AreEqual(result.Root.Name, sourceDoc.Root.Name);
            // that root element should have the xdt prefix namespace
            Assert.AreEqual(result.Root.GetPrefixOfNamespace(XdtVisitor._XdtNamespaceUri), "xdt");
            // that root element should be empty, as there are no differences
            Assert.IsFalse(result.Root.HasElements);
        }
    }
}
