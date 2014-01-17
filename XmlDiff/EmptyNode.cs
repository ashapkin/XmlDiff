using System;

namespace XmlDiff
{
	internal sealed class EmptyNode : INode
	{
		public DiffNode CompareWith(INode node)
		{
			return node.CompareWith(this);
		}

		public DiffNode CompareWith(EmptyNode node)
		{
			throw new InvalidOperationException();
		}

		public DiffNode CompareWith(RealNode node)
		{
			return new DiffNode(node.DefaultAction, node.Raw);
		}
	}
}
