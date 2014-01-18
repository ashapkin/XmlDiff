using XmlDiff.Visitors;

namespace XmlDiff
{
	public abstract class DiffContent
	{
		public virtual bool IsChanged { get { return true; } }

		public abstract void Accept(IDiffVisitor visitor);
		public abstract void Accept<T>(IDiffParamsVisitor<T> visitor, T param);
	}
}
