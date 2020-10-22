using System.Collections.Generic;
using System.IO;
using System.Xml;
using RimWorld;

namespace Verse
{
	public static class LanguageDataWriter
	{
		public static void WriteBackstoryFile()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.DevOutputFolderPath);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			if (new FileInfo(GenFilePaths.BackstoryOutputFilePath).Exists)
			{
				Find.WindowStack.Add(new Dialog_MessageBox("Cannot write: File already exists at " + GenFilePaths.BackstoryOutputFilePath));
				return;
			}
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "\t";
			using (XmlWriter xmlWriter = XmlWriter.Create(GenFilePaths.BackstoryOutputFilePath, xmlWriterSettings))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement("BackstoryTranslations");
				foreach (KeyValuePair<string, Backstory> allBackstory in BackstoryDatabase.allBackstories)
				{
					Backstory value = allBackstory.Value;
					xmlWriter.WriteStartElement(value.identifier);
					xmlWriter.WriteElementString("title", value.title);
					if (!value.titleFemale.NullOrEmpty())
					{
						xmlWriter.WriteElementString("titleFemale", value.titleFemale);
					}
					xmlWriter.WriteElementString("titleShort", value.titleShort);
					if (!value.titleShortFemale.NullOrEmpty())
					{
						xmlWriter.WriteElementString("titleShortFemale", value.titleShortFemale);
					}
					xmlWriter.WriteElementString("desc", value.baseDesc);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
			}
			Messages.Message("Fresh backstory translation file saved to " + GenFilePaths.BackstoryOutputFilePath, MessageTypeDefOf.NeutralEvent, historical: false);
		}
	}
}
