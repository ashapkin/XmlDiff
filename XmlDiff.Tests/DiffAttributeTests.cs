using System;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;

namespace XmlDiff.Tests
{
	[TestFixture]
	public class DiffAttributeTests
	{
		private readonly XAttribute Raw = new XAttribute("name", "val");

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_ShouldNotAllowRaw_ToBeNull()
		{
			new DiffAttribute(DiffAction.Added, null);
		}

		[Test]
		[TestCase(DiffAction.Added)]
		[TestCase(DiffAction.Removed)]
		public void IsChangedProperty_ShouldAlwaysBeTrue(DiffAction action)
		{
			var diffAttr = new DiffAttribute(action, Raw);
			Assert.IsTrue(diffAttr.IsChanged);
		}

		[Test]
		public void AppendSelfToSb_ReturnStringRepresentation_WithIndent()
		{
			var diffAttr = new DiffAttribute(DiffAction.Added, Raw);
			var sb = new StringBuilder();
			diffAttr.AppendSelfToSb(sb, 2);
			string result = sb.ToString();
			string expected = "......+ Attribute: \"name\" with value: \"val\"\r\n";
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void ToString_ReturnStringRepresentation_WithoutIndent()
		{
			var diffAttr = new DiffAttribute(DiffAction.Removed, Raw);
			string result = diffAttr.ToString();
			string expected = "- Attribute: \"name\" with value: \"val\"\r\n";
			Assert.AreEqual(expected, result);
		}
	}
}
