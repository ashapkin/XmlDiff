using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace XmlDiff.Tests
{
	[TestFixture]
	public class DiffNodeTests
	{
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DiffActionCtor_ShouldNotAllowRaw_ToBeNull()
		{
			new DiffNode(DiffAction.Added, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ChildsCtor_ShouldNotAllowRaw_ToBeNull()
		{
			new DiffNode(null, Enumerable.Empty<DiffContent>());
		}

		[Test]
		public void ChildsCtor_ShouldCreateEmptyChildlist_IfNullPassed()
		{
			var diffNode = new DiffNode(new XElement("some"), null);
			Assert.IsEmpty(diffNode.Content);
		}

		[Test]
		[TestCase(DiffAction.Added)]
		[TestCase(DiffAction.Removed)]
		public void IsChangedProperty_ShouldBeTrueIfDiffActionSpecified(DiffAction action)
		{
			var diffNode = new DiffNode(action, new XElement("some"));
			Assert.IsTrue(diffNode.IsChanged);
		}

		[Test]
		public void IsChangedProperty_ShouldBeFalseIfNoDiffActionAndChildsSpecified()
		{
			var diffNode = new DiffNode(new XElement("some"), null);
			Assert.IsFalse(diffNode.IsChanged);
		}

		[Test]
		public void IsChangedProperty_ShouldReturnTrueIfAnyOfChildsIsChanged()
		{
			var childs = new List<DiffContent>
			{
				new DiffNode(new XElement("child"), null),
				new DiffValue(DiffAction.Added, "value")
			};
			var diffNode = new DiffNode(new XElement("some"), childs);
			Assert.IsTrue(diffNode.IsChanged);
		}

		[Test]
		public void ToString_ReturnStringRepresentation()
		{
			var child = new DiffNode(DiffAction.Added, new XElement("child"));
			var diffAttr = new DiffNode(new XElement("root"), new DiffContent[] { child });
			string result = diffAttr.ToString();
			string expected = "= Element \"root\"\r\n" +
							  "...+ Element \"child\"\r\n";
			Assert.AreEqual(expected, result);
		}
	}
}
