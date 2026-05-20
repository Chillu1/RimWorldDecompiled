using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class ScenPart_IncidentBase : ScenPart
{
	protected IncidentDef incident;

	public IncidentDef Incident => incident;

	protected abstract string IncidentTag { get; }

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref incident, "incident");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && incident == null)
		{
			incident = RandomizableIncidents().FirstOrDefault();
			Log.Error("ScenPart has null incident after loading. Changing to " + incident.ToStringSafe());
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((incident != null) ? incident.GetHashCode() : 0);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
		DoIncidentEditInterface(scenPartRect);
	}

	public override string Summary(Scenario scen)
	{
		string key = "ScenPart_" + IncidentTag;
		return ScenSummaryList.SummaryWithList(scen, IncidentTag, key.Translate());
	}

	public override IEnumerable<string> GetSummaryListEntries(string tag)
	{
		if (tag == IncidentTag)
		{
			yield return incident.LabelCap;
		}
	}

	public override void Randomize()
	{
		incident = RandomizableIncidents().RandomElement();
	}

	public override bool TryMerge(ScenPart other)
	{
		if (other is ScenPart_IncidentBase scenPart_IncidentBase)
		{
			return scenPart_IncidentBase.Incident == incident;
		}
		return false;
	}

	public override bool CanCoexistWith(ScenPart other)
	{
		if (other is ScenPart_IncidentBase scenPart_IncidentBase)
		{
			return scenPart_IncidentBase.Incident != incident;
		}
		return true;
	}

	protected virtual IEnumerable<IncidentDef> RandomizableIncidents()
	{
		return Enumerable.Empty<IncidentDef>();
	}

	protected void DoIncidentEditInterface(Rect rect)
	{
		if (!Widgets.ButtonText(rect, incident.LabelCap))
		{
			return;
		}
		FloatMenuUtility.MakeMenu(DefDatabase<IncidentDef>.AllDefs, (IncidentDef id) => id.LabelCap, (IncidentDef id) => delegate
		{
			incident = id;
		});
	}
}
