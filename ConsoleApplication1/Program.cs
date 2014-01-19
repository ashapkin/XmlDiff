using System;
using System.IO;
using System.Xml.Linq;
using XmlDiff;
using XmlDiff.Visitors;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
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

			var comparer = new XmlComparer();
			var diffs = comparer.Compare(source, result);
			var htmlVisitor = new HtmlVisitor();
			htmlVisitor.Visit(diffs, 0);
			File.WriteAllText(string.Format("{0}.html", Guid.NewGuid()), htmlVisitor.Result);
		}
	}
}
