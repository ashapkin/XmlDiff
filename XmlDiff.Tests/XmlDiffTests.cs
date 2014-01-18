using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace XmlDiff.Tests
{
	[TestFixture]
	public class XmlDiffTests
	{
		private XmlComparer _xmlDiff = new XmlComparer();

		[Test]
		public void Compare_DifferentRootElements()
		{
			var sourceAttr = new XAttribute("value", 1);
			var source = new XElement("root", sourceAttr);
			var resultAttr = new XAttribute("value", 2);
			var result = new XElement("root", resultAttr);

			DiffNode output = _xmlDiff.Compare(source, result);
			Assert.AreEqual(result, output.Raw);
			verifyAttribute(DiffAction.Removed, output.Attributes, sourceAttr);
			verifyAttribute(DiffAction.Added, output.Attributes, resultAttr);
		}

		[Test]
		public void Compare_ComplexStructure()
		{
			var source =
				new XElement("config", new XAttribute("admin", true), new XAttribute("action", "compare"),
					new XElement("connection", new XAttribute("port", 123), new XAttribute("dataBase", "localhost"),
						new XElement("unity",
								new XElement("item", new XAttribute("name", "item1"), new XAttribute("value", -1)),
								new XElement("item", new XAttribute("name", "item2"), new XAttribute("value", 0), new XText("OldText")),
								new XElement("item", new XAttribute("name", "item3")),
								new XComment("Comment should be ignored"),
								new XElement("removed"))),
					new XElement("duplicate"),
					new XElement("duplicate", new XAttribute("a", true)),
					new XElement("unchanged",
						new XElement("unchanged",
							new XElement("removed"))));

			var result =
				new XElement("config", new XAttribute("admin", false),
					new XElement("connection", new XAttribute("port", 123), new XAttribute("dataBase", "localhost"),
						new XElement("unity",
								new XElement("item", new XAttribute("name", "item1"), new XAttribute("value", -1)),
								new XElement("item", new XAttribute("name", "item3"), new XText("NewText")),
								new XElement("added"))),
					new XElement("duplicate"),
					new XComment("Comment should be ignored"),
					new XElement("duplicate", new XAttribute("a", false)),
					new XElement("unchanged",
						new XElement("unchanged",
							new XElement("added"), new XAttribute("some", "attr"))));

			DiffNode configLevel = _xmlDiff.Compare(source, result);
			verifyAttribute(DiffAction.Removed, configLevel.Attributes, new XAttribute("admin", true));
			verifyAttribute(DiffAction.Removed, configLevel.Attributes, new XAttribute("action", "compare"));
			verifyAttribute(DiffAction.Added, configLevel.Attributes, new XAttribute("admin", false));

			DiffNode connectionLevel = configLevel.Childs.ElementAt(0);
			DiffNode unity = connectionLevel.Childs.ElementAt(0);
			DiffNode item2_level = unity.Childs.First();
			verifyAttribute(DiffAction.Removed, item2_level.Attributes, new XAttribute("name", "item2"));
			verifyAttribute(DiffAction.Removed, item2_level.Attributes, new XAttribute("value", 0));
			verifyAttribute(DiffAction.Added, item2_level.Attributes, new XAttribute("name", "item3"));

			verifyText(DiffAction.Removed, item2_level.Values, "OldText");
			verifyText(DiffAction.Added, item2_level.Values, "NewText");

			DiffNode unity_Removed = unity.Childs.SingleOrDefault(x => x.DiffAction == DiffAction.Removed && x.Raw.Name == "removed");
			DiffNode unity_item3 = unity.Childs.SingleOrDefault(x => x.DiffAction == DiffAction.Removed && x.Raw.Name == "item");
			DiffNode unity_Added = unity.Childs.SingleOrDefault(x => x.DiffAction == DiffAction.Added && x.Raw.Name == "added");

			DiffNode duplicate = configLevel.Childs.ElementAt(1);
			verifyAttribute(DiffAction.Removed, duplicate.Attributes, new XAttribute("a", true));
			verifyAttribute(DiffAction.Added, duplicate.Attributes, new XAttribute("a", false));

			DiffNode unchangedLevel0 = configLevel.Childs.ElementAt(2);
			DiffNode unchangedLevel1 = unchangedLevel0.Childs.Single();
			DiffNode unchangedLevel1_removed = unchangedLevel1.Childs.Single(x => x.DiffAction == DiffAction.Removed);
			Assert.AreEqual("removed", unchangedLevel1_removed.Raw.Name.ToString());
			DiffNode unchangedLevel1_added = unchangedLevel1.Childs.Single(x => x.DiffAction == DiffAction.Added);
			Assert.AreEqual("added", unchangedLevel1_added.Raw.Name.ToString());
		}

		[Test]
		public void Compare_SingleAttributeDeepChange()
		{
			var source =
				new XElement("level0",
					new XElement("level1",
						new XElement("level2"),
						new XElement("level2",
							new XElement("changed", new XAttribute("isProd", false))),
						new XElement("level2")));

			var newElem = new XElement("changed", new XAttribute("isProd", true));
			var result =
				new XElement("level0",
					new XElement("level1",
						new XElement("level2"),
						new XElement("level2",
							newElem),
						new XElement("level2")));

			DiffNode level0 = _xmlDiff.Compare(source, result);
			var level1 = level0.Childs.Single();
			var level2 = level1.Childs.Single();
			var changed = level2.Childs.Single();
			Assert.AreEqual(newElem, changed.Raw);
			verifyAttribute(DiffAction.Removed, changed.Attributes, new XAttribute("isProd", false));
			verifyAttribute(DiffAction.Added, changed.Attributes, new XAttribute("isProd", true));
		}

		[Test]
		public void Compare_SingleTextDeepChange()
		{
			var source =
				new XElement("level0",
					new XElement("level1",
						new XElement("level2"),
						new XElement("level2",
							new XElement("isProd", new XText("true"), new XElement("someelem"), new XText("!Bang"))),
						new XElement("level2")));

			var result =
				new XElement("level0",
					new XElement("level1",
						new XElement("level2"),
						new XElement("level2",
							new XElement("isProd", new XText("false"), new XElement("someelem"), new XText("!Bang"))),
						new XElement("level2")));

			DiffNode level0 = _xmlDiff.Compare(source, result);
			var level1 = level0.Childs.Single();
			var level2 = level1.Childs.Single();
			var changed = level2.Childs.Single();
			verifyText(DiffAction.Removed, changed.Values, "true!Bang");
			verifyText(DiffAction.Added, changed.Values, "false!Bang");
		}

		[Test]
		public void Compare_SingleText_NotAffectNestedElements()
		{
			var source =
				new XElement("level0",
					new XElement("isProd", new XText("true"),
						new XElement("level2"),
						new XElement("level2", new XText("level2Text"),
							new XElement("level3", new XText("level3Text"))),
						new XElement("level2")));

			var result =
				new XElement("level0",
					new XElement("isProd", new XText("false"),
						new XElement("level2"),
						new XElement("level2", new XText("level2Text"),
							new XElement("level3", new XText("level3Text"))),
						new XElement("level2")));

			DiffNode level0 = _xmlDiff.Compare(source, result);
			var isProdLevel = level0.Childs.Single();
			Assert.AreEqual(2, isProdLevel.Values.Count());
			Assert.AreEqual(0, isProdLevel.Childs.Count());
			verifyText(DiffAction.Removed, isProdLevel.Values, "true");
			verifyText(DiffAction.Added, isProdLevel.Values, "false");
		}

		private void verifyAttribute(DiffAction action, IEnumerable<DiffAttribute> attrs, XAttribute expected)
		{
			var raws = attrs.Where(x => x.Action == action).Select(x => x.Raw);
			Assert.IsTrue(raws.Any(x => x.Name == expected.Name && x.Value == expected.Value));
		}

		private void verifyText(DiffAction action, IEnumerable<DiffValue> texts, string expected)
		{
			var raws = texts.Where(x => x.Action == action).Select(x => x.Raw);
			Assert.IsTrue(raws.Any(x => x == expected));
		}
	}
}
