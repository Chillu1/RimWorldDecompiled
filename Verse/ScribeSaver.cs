using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Verse;

public class ScribeSaver
{
	public DebugLoadIDsSavingErrorsChecker loadIDsErrorsChecker = new DebugLoadIDsSavingErrorsChecker();

	public bool savingForDebug;

	private Stream saveStream;

	private XmlWriter writer;

	private string curPath;

	private HashSet<string> savedNodes = new HashSet<string>();

	private int nextListElementTemporaryId;

	private bool anyInternalException;

	public string CurPath => curPath;

	public void InitSaving(string filePath, string documentElementName)
	{
		if (Scribe.mode != LoadSaveMode.Inactive)
		{
			Log.Error("Called InitSaving() but current mode is " + Scribe.mode);
			Scribe.ForceStop();
		}
		if (curPath != null)
		{
			Log.Error("Current path is not null in InitSaving");
			curPath = null;
			savedNodes.Clear();
			nextListElementTemporaryId = 0;
		}
		try
		{
			Scribe.mode = LoadSaveMode.Saving;
			saveStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "\t";
			writer = XmlWriter.Create(saveStream, xmlWriterSettings);
			writer.WriteStartDocument();
			EnterNode(documentElementName);
		}
		catch (Exception ex)
		{
			Log.Error("Exception while init saving file: " + filePath + "\n" + ex);
			ForceStop();
			throw;
		}
	}

	public void FinalizeSaving()
	{
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			Log.Error("Called FinalizeSaving() but current mode is " + Scribe.mode);
			return;
		}
		if (anyInternalException)
		{
			ForceStop();
			throw new Exception("Can't finalize saving due to internal exception. The whole file would be most likely corrupted anyway.");
		}
		try
		{
			if (writer != null)
			{
				ExitNode();
				writer.WriteEndDocument();
				writer.Flush();
				writer.Close();
				writer = null;
			}
			if (saveStream != null)
			{
				saveStream.Flush();
				saveStream.Close();
				saveStream = null;
			}
			Scribe.mode = LoadSaveMode.Inactive;
			savingForDebug = false;
			loadIDsErrorsChecker.CheckForErrorsAndClear();
			curPath = null;
			savedNodes.Clear();
			nextListElementTemporaryId = 0;
			anyInternalException = false;
		}
		catch (Exception ex)
		{
			Log.Error("Exception in FinalizeLoading(): " + ex);
			ForceStop();
			throw;
		}
	}

	public void WriteElement(string elementName, string value)
	{
		if (writer == null)
		{
			Log.Error("Called WriteElemenet(), but writer is null.");
			return;
		}
		try
		{
			writer.WriteElementString(elementName, value);
		}
		catch (Exception)
		{
			anyInternalException = true;
			throw;
		}
	}

	public void WriteAttribute(string attributeName, string value)
	{
		if (writer == null)
		{
			Log.Error("Called WriteAttribute(), but writer is null.");
			return;
		}
		try
		{
			writer.WriteAttributeString(attributeName, value);
		}
		catch (Exception)
		{
			anyInternalException = true;
			throw;
		}
	}

	public string DebugOutputFor(IExposable saveable)
	{
		if (Scribe.mode != LoadSaveMode.Inactive)
		{
			Log.Error("DebugOutput needs current mode to be Inactive");
			return "";
		}
		try
		{
			using StringWriter stringWriter = new StringWriter();
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "  ";
			xmlWriterSettings.OmitXmlDeclaration = true;
			try
			{
				using (writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
				{
					Scribe.mode = LoadSaveMode.Saving;
					savingForDebug = true;
					Scribe_Deep.Look(ref saveable, "saveable");
				}
				return stringWriter.ToString();
			}
			finally
			{
				ForceStop();
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception while getting debug output: " + ex);
			ForceStop();
			return "";
		}
	}

	public bool EnterNode(string nodeName)
	{
		if (writer == null)
		{
			return false;
		}
		try
		{
			writer.WriteStartElement(nodeName);
		}
		catch (Exception)
		{
			anyInternalException = true;
			throw;
		}
		return true;
	}

	public void ExitNode()
	{
		if (writer == null)
		{
			return;
		}
		try
		{
			writer.WriteEndElement();
		}
		catch (Exception)
		{
			anyInternalException = true;
			throw;
		}
	}

	public void ForceStop()
	{
		if (writer != null)
		{
			writer.Close();
			writer = null;
		}
		if (saveStream != null)
		{
			saveStream.Close();
			saveStream = null;
		}
		savingForDebug = false;
		loadIDsErrorsChecker.Clear();
		curPath = null;
		savedNodes.Clear();
		nextListElementTemporaryId = 0;
		anyInternalException = false;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			Scribe.mode = LoadSaveMode.Inactive;
		}
	}
}
