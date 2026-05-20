using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class GeneSetHolderBase : ThingWithComps
	{
		protected GeneSet geneSet;

		private const int MaxGeneLabels = 5;

		public static readonly CachedTexture GeneticInfoTex = new CachedTexture("UI/Gizmos/ViewGenes");

		private List<string> tmpGeneLabelsDesc = new List<string>();

		private List<string> tmpGeneLabels = new List<string>();

		public GeneSet GeneSet => geneSet;

		protected virtual string InspectGeneDescription => "InspectGenesDesc".Translate(this);

		public override string DescriptionDetailed
		{
			get
			{
				tmpGeneLabelsDesc.Clear();
				string text = base.DescriptionDetailed;
				if (geneSet == null || !geneSet.GenesListForReading.Any())
				{
					return text;
				}
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				for (int i = 0; i < geneSet.GenesListForReading.Count; i++)
				{
					tmpGeneLabelsDesc.Add(geneSet.GenesListForReading[i].label);
				}
				return text + ("Genes".Translate().CapitalizeFirst() + ":\n" + tmpGeneLabelsDesc.ToLineList("  - ", capitalizeItems: true));
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats())
			{
				yield return item;
			}
			if (geneSet == null)
			{
				yield break;
			}
			Dialog_InfoCard.Hyperlink? inspectGenesHyperlink = null;
			if (ThingSelectionUtility.SelectableByMapClick(this))
			{
				inspectGenesHyperlink = new Dialog_InfoCard.Hyperlink(this, -1, thingIsGeneOwner: true);
			}
			foreach (StatDrawEntry item2 in geneSet.SpecialDisplayStats(inspectGenesHyperlink))
			{
				yield return item2;
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			tmpGeneLabels.Clear();
			if (geneSet != null && geneSet.GenesListForReading.Any())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				List<GeneDef> genesListForReading = geneSet.GenesListForReading;
				int num = Mathf.Min(5, genesListForReading.Count);
				for (int i = 0; i < num; i++)
				{
					string text2 = genesListForReading[i].label;
					if (geneSet.IsOverridden(genesListForReading[i]))
					{
						text2 += " (" + "Overridden".Translate() + ")";
					}
					tmpGeneLabels.Add(text2);
				}
				if (genesListForReading.Count > num)
				{
					tmpGeneLabels.Add("Etc".Translate() + "...");
				}
				text += "Genes".Translate().CapitalizeFirst() + ":\n" + tmpGeneLabels.ToLineList("  - ", capitalizeItems: true);
			}
			return text;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (geneSet != null)
			{
				yield return new Command_Action
				{
					defaultLabel = "InspectGenes".Translate() + "...",
					defaultDesc = InspectGeneDescription,
					icon = GeneticInfoTex.Texture,
					action = delegate
					{
						InspectPaneUtility.OpenTab(typeof(ITab_Genes));
					}
				};
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref geneSet, "geneSet");
		}
	}
}
