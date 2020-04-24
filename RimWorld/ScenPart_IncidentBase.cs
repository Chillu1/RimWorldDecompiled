using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_IncidentBase : ScenPart
	{
		protected IncidentDef incident;

		public IncidentDef Incident => incident;

		protected abstract string IncidentTag
		{
			get;
		}

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
			ScenPart_IncidentBase scenPart_IncidentBase = other as ScenPart_IncidentBase;
			if (scenPart_IncidentBase != null && scenPart_IncidentBase.Incident == incident)
			{
				return true;
			}
			return false;
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_IncidentBase scenPart_IncidentBase = other as ScenPart_IncidentBase;
			if (scenPart_IncidentBase != null && scenPart_IncidentBase.Incident == incident)
			{
				return false;
			}
			return true;
		}

		protected virtual IEnumerable<IncidentDef> RandomizableIncidents()
		{
			return Enumerable.Empty<IncidentDef>();
		}

		protected void DoIncidentEditInterface(Rect rect)
		{
			if (Widgets.ButtonText(rect, incident.LabelCap))
			{
				FloatMenuUtility.MakeMenu(DefDatabase<IncidentDef>.AllDefs, (IncidentDef id) => id.LabelCap, delegate(IncidentDef id)
				{
					ScenPart_IncidentBase scenPart_IncidentBase = this;
					return delegate
					{
						scenPart_IncidentBase.incident = id;
					};
				});
			}
		}
	}
}
