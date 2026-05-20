using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class BookOutcomeProperties_GainResearch : BookOutcomeProperties
{
	public class BookResearchItem
	{
		public ResearchProjectDef project;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "project", xmlRoot.Name);
		}
	}

	public class BookTabItem
	{
		public ResearchTabDef tab;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "tab", xmlRoot.Name);
		}
	}

	public List<BookResearchItem> include = new List<BookResearchItem>();

	public List<BookResearchItem> exclude = new List<BookResearchItem>();

	public ResearchTabDef tab;

	public List<BookTabItem> tabs = new List<BookTabItem>();

	public bool ignoreZeroBaseCost = true;

	public bool usesHiddenProjects;

	public override Type DoerClass => typeof(ReadingOutcomeDoerGainResearch);

	public void PostLoad()
	{
		if (tab != null)
		{
			tabs.Add(new BookTabItem
			{
				tab = tab
			});
		}
	}
}
