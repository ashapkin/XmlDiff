
namespace XmlDiff.Visitors
{
	public interface IDiffParamsVisitor<T>
	{
		T Initial { get; }
		void Visit(DiffAttribute attr, T param);
		void Visit(DiffValue val, T param);
		void Visit(DiffNode node, T param);
		void Visit(DiffNode node);
		string Result { get; }
	}
}
