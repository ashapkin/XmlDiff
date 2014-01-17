using System.Text;

namespace XmlDiff
{
	public abstract class DiffContent
	{
		public virtual bool IsChanged { get { return true; } }

		public override string ToString()
		{
			var sb = new StringBuilder();
			AppendSelfToSb(sb, 0);
			return sb.ToString();
		}

		protected internal abstract void AppendSelfToSb(StringBuilder sb, int level);

		protected static string BuildIndent(int level)
		{
			var sb = new StringBuilder();
			while (level > 0)
			{
				sb.Append("...");
				level--;
			}
			return sb.ToString();
		}

		protected static string ActionToString(DiffAction? diffAction)
		{
			if (diffAction == DiffAction.Added)
			{
				return "+";
			}
			if (diffAction == DiffAction.Removed)
			{
				return "-";
			}

			return "=";
		}
	}
}
