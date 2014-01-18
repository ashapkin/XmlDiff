using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiff.Visitors
{
	public class HtmlVisitor : IDiffParamsVisitor<int>
	{
		public void Visit(DiffAttribute attr, int param)
		{
			throw new NotImplementedException();
		}

		public void Visit(DiffValue val, int param)
		{
			throw new NotImplementedException();
		}

		public void Visit(DiffNode node, int param)
		{
			throw new NotImplementedException();
		}
	}
}
