using System;
using System.Xml.Linq;

namespace XmlDiff
{
	internal sealed class IndexedName : IEquatable<IndexedName>
	{
		public IndexedName(XName name, int index)
		{
			Name = name;
			Index = index;
		}
		public XName Name { get; private set; }
		public int Index { get; private set; }

		public bool Equals(IndexedName other)
		{
			return Object.ReferenceEquals(null, other)
				? false
				: Name == other.Name && Index == other.Index;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as IndexedName);
		}

		public override int GetHashCode()
		{
			return 7 * Name.GetHashCode() ^ Index.GetHashCode();
		}
	}
}
