using System;
using System.IO;
using System.Xml;

namespace Verse
{
	public class LoadableXmlAsset
	{
		private static XmlReader reader;

		public string name;

		public string fullFolderPath;

		public XmlDocument xmlDoc;

		public ModContentPack mod;

		public string FullFilePath => fullFolderPath + Path.DirectorySeparatorChar + name;

		public LoadableXmlAsset(string name, string fullFolderPath, string contents)
		{
			this.name = name;
			this.fullFolderPath = fullFolderPath;
			try
			{
				XmlReaderSettings settings = new XmlReaderSettings
				{
					IgnoreComments = true,
					IgnoreWhitespace = true,
					CheckCharacters = false
				};
				using StringReader input = new StringReader(contents);
				using XmlReader xmlReader = XmlReader.Create(input, settings);
				xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlReader);
			}
			catch (Exception ex)
			{
				Log.Warning("Exception reading " + name + " as XML: " + ex);
				xmlDoc = null;
			}
		}

		public override string ToString()
		{
			return name;
		}
	}
}
