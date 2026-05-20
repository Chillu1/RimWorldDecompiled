using System.IO;
using System.Xml;
using RimWorld;

namespace Verse;

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
			foreach (BackstoryDef allDef in DefDatabase<BackstoryDef>.AllDefs)
			{
				xmlWriter.WriteStartElement(allDef.identifier);
				xmlWriter.WriteElementString("title", allDef.title);
				if (!allDef.titleFemale.NullOrEmpty())
				{
					xmlWriter.WriteElementString("titleFemale", allDef.titleFemale);
				}
				xmlWriter.WriteElementString("titleShort", allDef.titleShort);
				if (!allDef.titleShortFemale.NullOrEmpty())
				{
					xmlWriter.WriteElementString("titleShortFemale", allDef.titleShortFemale);
				}
				xmlWriter.WriteElementString("desc", allDef.description);
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}
		Messages.Message("Fresh backstory translation file saved to " + GenFilePaths.BackstoryOutputFilePath, MessageTypeDefOf.NeutralEvent, historical: false);
	}
}
