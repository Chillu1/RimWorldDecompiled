using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimWorld.IO;

public static class VirtualFileInfoExt
{
	public static XDocument LoadAsXDocument(this VirtualFile file)
	{
		using Stream input = file.CreateReadStream();
		return XDocument.Load(XmlReader.Create(input), LoadOptions.SetLineInfo);
	}

	public static XDocument LoadAsXDocument(string fileContents)
	{
		using StringReader textReader = new StringReader(fileContents);
		return XDocument.Load(textReader, LoadOptions.SetLineInfo);
	}
}
