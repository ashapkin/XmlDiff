using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XmlDiff
{
	internal sealed class RealNode : INode
	{
		public RealNode(DiffAction defaultAction, XElement raw,
			Dictionary<XName, XAttribute> attrs, Dictionary<IndexedName, RealNode> childs)
		{
			DefaultAction = defaultAction;
			Raw = raw;
			Childs = childs;
			Attributes = attrs;
		}

		public XElement Raw { get; private set; }
		public Dictionary<XName, XAttribute> Attributes { get; private set; }
		public Dictionary<IndexedName, RealNode> Childs { get; private set; }
		public DiffAction DefaultAction { get; private set; }

		public DiffNode CompareWith(INode node)
		{
			return node.CompareWith(this);
		}

		public DiffNode CompareWith(EmptyNode node)
		{
			return new DiffNode(DefaultAction, Raw);
		}

		public DiffNode CompareWith(RealNode node)
		{
			//swap nodes to be sure that raw element with default action = Added comes first
			//since we are looking for a comparison against a current version
			if (DefaultAction == DiffAction.Removed)
			{
				return node.CompareWith(this);
			}

			var contentChanges = attributeChanges(node)
				.Concat(valueChanges(node))
				.Concat(childsChanges(node));
			return new DiffNode(Raw, contentChanges);
		}

		private IEnumerable<DiffContent> attributeChanges(RealNode node)
		{
			var pair = Pair.Create(node.Attributes, node.DefaultAction, Attributes, DefaultAction);
			return pair.Apply((source, result, action) =>
				{
					return source
						.Where(x => !result.ContainsKey(x.Key) || result[x.Key].ToString() != x.Value.ToString())
						.Select(x => new DiffAttribute(action, x.Value));
				});
		}

		private IEnumerable<DiffContent> valueChanges(RealNode node)
		{
			var pair = Pair.Create(getTextValue(node.Raw), node.DefaultAction, getTextValue(Raw), DefaultAction);
			return pair.Apply((source, result, action) =>
				{
					return source != result && !string.IsNullOrEmpty(source)
						? new DiffValue[] { new DiffValue(action, source) }
						: new DiffValue[0];
				});
		}

		private static string getTextValue(XElement elem)
		{
			return string.Join("", elem.Nodes().OfType<XText>().Select(x => x.Value));
		}

		private IEnumerable<DiffContent> childsChanges(RealNode node)
		{
			var childs = zip(node.Childs, Childs)
				.Select(x => x.Result.CompareWith(x.Source))
				.Where(x => x.IsChanged);
			return childs;
		}

		private static IEnumerable<Pair<INode>> zip(
			Dictionary<IndexedName, RealNode> source, Dictionary<IndexedName, RealNode> result)
		{
			foreach (var sourcePair in source)
			{
				RealNode targetValue;
				yield return result.TryGetValue(sourcePair.Key, out targetValue)
					? new Pair<INode>(sourcePair.Value, targetValue)
					: new Pair<INode>(sourcePair.Value, new EmptyNode());
			}
			foreach (var pair in result.Where(x => !source.ContainsKey(x.Key)))
			{
				yield return new Pair<INode>(new EmptyNode(), pair.Value);
			}
		}

		private sealed class Pair<T>
		{
			public Pair(T source, T result)
			{
				Source = source;
				Result = result;
			}

			public Pair(T source, DiffAction sourceAction, T result, DiffAction resultAction)
				: this(source, result)
			{
				SourceAction = sourceAction;
				ResultAction = resultAction;
			}

			public IEnumerable<R> Apply<R>(Func<T, T, DiffAction, IEnumerable<R>> projection)
			{
				return projection(Source, Result, SourceAction).Concat(projection(Result, Source, ResultAction));
			}

			public DiffAction SourceAction { get; private set; }
			public DiffAction ResultAction { get; private set; }
			public T Source { get; private set; }
			public T Result { get; private set; }
		}

		private sealed class Pair
		{
			static Pair() { }

			public static Pair<T> Create<T>(T source, DiffAction sourceAction, T result, DiffAction resultAction)
			{
				return new Pair<T>(source, sourceAction, result, resultAction);
			}
		}
	}
}
