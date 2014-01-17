
namespace XmlDiff
{
	internal interface INode
	{
		DiffNode CompareWith(INode node);
		DiffNode CompareWith(EmptyNode node);
		DiffNode CompareWith(RealNode node);
	}
}
