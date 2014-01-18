using System.Xml.Linq;

namespace XmlDiff
{
	public interface IXmlComparer
	{
		DiffNode Compare(XElement sourceElement, XElement resultElement);
	}
}
