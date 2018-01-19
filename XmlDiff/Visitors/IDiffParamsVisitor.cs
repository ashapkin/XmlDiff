
namespace XmlDiff.Visitors
{
	public interface IDiffParamsVisitor<T>
	{
		void Visit(DiffAttribute attr, T param);
		void Visit(DiffValue val, T param);
		void Visit(DiffNode node, T param);
		void VisitWithDefaultSettings(DiffNode node);
		string Result { get; }
	}
}
