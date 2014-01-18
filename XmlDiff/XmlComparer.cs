using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XmlDiff
{
	public class XmlComparer : IXmlComparer
	{
		/// <summary>
		/// Compare <paramref name="resultElement"/> with a <paramref name="sourceElement"/> 
		/// Comparison is made only for XElement, XAttribute or XText elements
		/// All other xml elements such as XComment, CDATA etc would be ignored
		/// </summary>
		/// <param name="sourceElement">Source element</param>
		/// <param name="resultElement">Result element</param>
		/// <returns>
		/// Return value is build upon <paramref name="resultElement"/>
		/// In case of different root tags InvalidOperationException is throws
		/// </returns>
		public DiffNode Compare(XElement sourceElement, XElement resultElement)
		{
			if (sourceElement == null)
			{
				throw new ArgumentNullException("sourceElement");
			}
			if (resultElement == null)
			{
				throw new ArgumentNullException("resultElement");
			}
			if (sourceElement.Name != resultElement.Name)
			{
				throw new InvalidOperationException("Root elements are different");
			}

			RealNode parsedSource = parse(sourceElement, DiffAction.Removed);
			RealNode parsedResult = parse(resultElement, DiffAction.Added);
			return parsedResult.CompareWith(parsedSource);
		}

		private static RealNode parse(XElement elem, DiffAction defaultAction)
		{
			Dictionary<IndexedName, RealNode> childs = elem.HasElements
				? parseChilds(elem, defaultAction)
				: new Dictionary<IndexedName, RealNode>();
			Dictionary<XName, XAttribute> attributes = elem.Attributes().ToDictionary(x => x.Name, x => x);
			return new RealNode(defaultAction, elem, getTextValue(elem), attributes, childs);
		}

		private static Dictionary<IndexedName, RealNode> parseChilds(XElement elem, DiffAction defaultAction)
		{
			return elem.Elements()
					.Select(x => parse(x, defaultAction))
					.GroupBy(x => x.Raw.Name)
					.Aggregate(Enumerable.Empty<KeyValuePair<IndexedName, RealNode>>(), reduce_group)
					.ToDictionary(x => x.Key, x => x.Value);
		}

		private static IEnumerable<KeyValuePair<IndexedName, RealNode>> reduce_group(
			IEnumerable<KeyValuePair<IndexedName, RealNode>> aggr, IGrouping<XName, RealNode> items)
		{
			var addition = items.Select((elem, i) =>
				new KeyValuePair<IndexedName, RealNode>(new IndexedName(items.Key, i), elem));
			return aggr.Concat(addition);
		}

		private static string getTextValue(XElement elem)
		{
			return string.Join("", elem.Nodes().OfType<XText>().Select(x => x.Value));
		}
	}
}