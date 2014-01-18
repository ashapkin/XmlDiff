
namespace XmlDiff.Visitors
{
	public interface IDiffVisitor
	{
		void Visit(DiffAttribute attr);
		void Visit(DiffValue val);
		void Visit(DiffNode node);
	}
}
