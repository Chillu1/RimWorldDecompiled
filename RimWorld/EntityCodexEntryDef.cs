using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class EntityCodexEntryDef : Def
{
	public EntityCategoryDef category;

	public bool startDiscovered;

	public List<ThingDef> linkedThings = new List<ThingDef>();

	private List<IncidentDef> linkedIncidents = new List<IncidentDef>();

	public List<ResearchProjectDef> discoveredResearchProjects = new List<ResearchProjectDef>();

	public List<IncidentDef> provocationIncidents = new List<IncidentDef>();

	public EntityDiscoveryType discoveryType;

	public bool allowDiscoveryWhileMapGenerating;

	public int orderInCategory = 9999999;

	public List<AnomalyPlaystyleDef> hideInPlaystyles = new List<AnomalyPlaystyleDef>();

	private ThingDef useDescriptionFrom;

	[NoTranslate]
	private string uiIconPath;

	public Texture2D icon;

	public Texture2D silhouette;

	private const string SilhouetteTexPathSuffix = "_Silhouette";

	public string Description
	{
		get
		{
			if (useDescriptionFrom == null)
			{
				return description;
			}
			return useDescriptionFrom.description;
		}
	}

	public bool Discovered => Find.EntityCodex.Discovered(this);

	public bool Visible
	{
		get
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				return !hideInPlaystyles.Contains(Find.Storyteller.difficulty.AnomalyPlaystyleDef);
			}
			return true;
		}
	}

	public override void PostLoad()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			icon = (uiIconPath.NullOrEmpty() ? BaseContent.BadTex : ContentFinder<Texture2D>.Get(uiIconPath));
			silhouette = (uiIconPath.NullOrEmpty() ? BaseContent.BadTex : ContentFinder<Texture2D>.Get(uiIconPath + "_Silhouette"));
		});
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		foreach (ThingDef linkedThing in linkedThings)
		{
			if (linkedThing.entityCodexEntry == null)
			{
				linkedThing.entityCodexEntry = this;
				continue;
			}
			Log.Error("EntityCodexEntryDef " + defName + " is linked to " + linkedThing.defName + " but " + linkedThing.defName + " is already linked to " + linkedThing.entityCodexEntry.defName + ".");
		}
		foreach (IncidentDef linkedIncident in linkedIncidents)
		{
			if (linkedIncident.codexEntry == null)
			{
				linkedIncident.codexEntry = this;
				continue;
			}
			Log.Error("EntityCodexEntryDef " + defName + " is linked to " + linkedIncident.defName + " but " + linkedIncident.defName + " is already linked to " + linkedIncident.codexEntry.defName + ".");
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (category == null)
		{
			yield return "category is null.";
		}
		if (uiIconPath.NullOrEmpty())
		{
			yield return "missing icon.";
		}
	}
}
