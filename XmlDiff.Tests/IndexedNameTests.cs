using System.Xml.Linq;
using NUnit.Framework;

namespace XmlDiff.Tests
{
	[TestFixture]
	public class IndexedNameTests
	{
		private readonly IndexedName Source = new IndexedName(XName.Get("name"), 4);

		[Test]
		public void Equals_ForNull_ReturnsFalse()
		{
			Assert.IsFalse(Source.Equals(null));
		}

		[Test]
		public void Equals_ForDifferentObject_ReturnsFalse()
		{
			Assert.IsFalse(Source.Equals(new object()));
		}

		[Test]
		public void Equals_ForTheSameXName_But_DifferentIndex_ReutnsFalse()
		{
			var another = new IndexedName(XName.Get("name"), 1);
			Assert.IsFalse(another.Equals(Source) || Source.Equals(another));
		}

		[Test]
		public void Equals_ForTheSameIndex_But_DifferentName_ReturnsFalse()
		{
			var another = new IndexedName(XName.Get("another"), 4);
			Assert.IsFalse(another.Equals(Source) || Source.Equals(another));
		}

		[Test]
		public void Equals_ForTheSameXName_And_IndexReturnsTrue()
		{
			var another = new IndexedName(XName.Get("name"), 4);
			Assert.IsTrue(Source.Equals(another) && another.Equals(Source));
		}
	}
}
