using System.Xml.Linq;

namespace XmlDiff
{
	public interface IXmlComparer
	{
		DiffNode Compare(XElement source, XElement result);
	}
}
