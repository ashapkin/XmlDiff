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
				throw new ArgumentNullException(nameof(sourceElement));
			}
			if (resultElement == null)
			{
				throw new ArgumentNullException(nameof(resultElement));
			}
			if (sourceElement.Name != resultElement.Name)
			{
				throw new InvalidOperationException("Root elements are different");
			}

			RealNode parsedSource = Parse(sourceElement, DiffAction.Removed);
			RealNode parsedResult = Parse(resultElement, DiffAction.Added);
			return parsedResult.CompareWith(parsedSource);
		}

		private static RealNode Parse(XElement elem, DiffAction defaultAction)
		{
			Dictionary<IndexedName, RealNode> childs = elem.HasElements
				? ParseChilds(elem, defaultAction)
				: new Dictionary<IndexedName, RealNode>();
			Dictionary<XName, XAttribute> attributes = elem.Attributes().ToDictionary(x => x.Name, x => x);
			return new RealNode(defaultAction, elem, GetTextValue(elem), attributes, childs);
		}

		private static Dictionary<IndexedName, RealNode> ParseChilds(XElement elem, DiffAction defaultAction)
		{
			return elem.Elements()
					.Select(x => Parse(x, defaultAction))
					.GroupBy(x => x.Raw.Name)
					.Aggregate(Enumerable.Empty<KeyValuePair<IndexedName, RealNode>>(), ReduceGroup)
					.ToDictionary(x => x.Key, x => x.Value);
		}

		private static IEnumerable<KeyValuePair<IndexedName, RealNode>> ReduceGroup(
			IEnumerable<KeyValuePair<IndexedName, RealNode>> aggr, IGrouping<XName, RealNode> items)
		{
			var addition = items.Select((elem, i) =>
				new KeyValuePair<IndexedName, RealNode>(new IndexedName(items.Key, i), elem));
			return aggr.Concat(addition);
		}

		private static string GetTextValue(XElement elem)
		{
			return string.Join("", elem.Nodes().OfType<XText>().Select(x => x.Value));
		}
	}
}