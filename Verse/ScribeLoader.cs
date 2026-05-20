using System;
using System.IO;
using System.Xml;

namespace Verse;

public class ScribeLoader
{
	public CrossRefHandler crossRefs = new CrossRefHandler();

	public PostLoadIniter initer = new PostLoadIniter();

	public IExposable curParent;

	public XmlNode curXmlParent;

	public string curPathRelToParent;

	public void InitLoading(string filePath)
	{
		if (Scribe.mode != LoadSaveMode.Inactive)
		{
			Log.Error("Called InitLoading() but current mode is " + Scribe.mode);
			Scribe.ForceStop();
		}
		if (curParent != null)
		{
			Log.Error("Current parent is not null in InitLoading");
			curParent = null;
		}
		if (curPathRelToParent != null)
		{
			Log.Error("Current path relative to parent is not null in InitLoading");
			curPathRelToParent = null;
		}
		try
		{
			using (StreamReader input = new StreamReader(filePath))
			{
				using XmlTextReader reader = new XmlTextReader(input);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(reader);
				curXmlParent = xmlDocument.DocumentElement;
			}
			Scribe.mode = LoadSaveMode.LoadingVars;
		}
		catch (Exception ex)
		{
			Log.Error("Exception while init loading file: " + filePath + "\n" + ex);
			ForceStop();
			throw;
		}
	}

	public void InitLoadingMetaHeaderOnly(string filePath)
	{
		if (Scribe.mode != LoadSaveMode.Inactive)
		{
			Log.Error("Called InitLoadingMetaHeaderOnly() but current mode is " + Scribe.mode);
			Scribe.ForceStop();
		}
		try
		{
			using (StreamReader input = new StreamReader(filePath))
			{
				using XmlTextReader xmlTextReader = new XmlTextReader(input);
				if (!ScribeMetaHeaderUtility.ReadToMetaElement(xmlTextReader))
				{
					return;
				}
				using XmlReader reader = xmlTextReader.ReadSubtree();
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(reader);
				XmlElement xmlElement = xmlDocument.CreateElement("root");
				xmlElement.AppendChild(xmlDocument.DocumentElement);
				curXmlParent = xmlElement;
			}
			Scribe.mode = LoadSaveMode.LoadingVars;
		}
		catch (Exception ex)
		{
			Log.Error("Exception while init loading meta header: " + filePath + "\n" + ex);
			ForceStop();
			throw;
		}
	}

	public void FinalizeLoading()
	{
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			Log.Error("Called FinalizeLoading() but current mode is " + Scribe.mode);
			return;
		}
		try
		{
			Scribe.ExitNode();
			curXmlParent = null;
			curParent = null;
			curPathRelToParent = null;
			Scribe.mode = LoadSaveMode.Inactive;
			DeepProfiler.Start("ResolveAllCrossReferences()");
			crossRefs.ResolveAllCrossReferences();
			DeepProfiler.End();
			DeepProfiler.Start("DoAllPostLoadInits()");
			initer.DoAllPostLoadInits();
			DeepProfiler.End();
		}
		catch (Exception ex)
		{
			Log.Error("Exception in FinalizeLoading(): " + ex);
			ForceStop();
			throw;
		}
	}

	public bool EnterNode(string nodeName)
	{
		if (curXmlParent != null)
		{
			XmlNode xmlNode = curXmlParent[nodeName];
			if (xmlNode == null && char.IsDigit(nodeName[0]))
			{
				xmlNode = curXmlParent.ChildNodes[int.Parse(nodeName)];
			}
			if (xmlNode == null)
			{
				return false;
			}
			curXmlParent = xmlNode;
		}
		curPathRelToParent = curPathRelToParent + "/" + nodeName;
		return true;
	}

	public void ExitNode()
	{
		if (curXmlParent != null)
		{
			curXmlParent = curXmlParent.ParentNode;
		}
		if (curPathRelToParent != null)
		{
			int num = curPathRelToParent.LastIndexOf('/');
			curPathRelToParent = ((num > 0) ? curPathRelToParent.Substring(0, num) : null);
		}
	}

	public void ForceStop()
	{
		curXmlParent = null;
		curParent = null;
		curPathRelToParent = null;
		crossRefs.Clear(errorIfNotEmpty: false);
		initer.Clear();
		if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			Scribe.mode = LoadSaveMode.Inactive;
		}
	}
}
