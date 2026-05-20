using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompHasSources : ThingComp
	{
		private List<string> sources = new List<string>();

		private const int MaxSourcesToDisplayInInspectString = 9;

		private List<string> tmpSourceLabels = new List<string>();

		public CompProperties_HasSources Props => (CompProperties_HasSources)props;

		public void AddSource(string sourceLabel)
		{
			if (!sourceLabel.NullOrEmpty() && !sources.Contains(sourceLabel))
			{
				sources.Add(sourceLabel);
			}
		}

		public void TransferSourcesTo(CompHasSources other, int count = -1)
		{
			if (count < 0)
			{
				count = sources.Count;
			}
			for (int i = 0; i < count; i++)
			{
				if (!sources.TryRandomElement(out var result))
				{
					break;
				}
				other.AddSource(result);
				sources.Remove(result);
			}
		}

		public override string TransformLabel(string label)
		{
			if (!Props.affectLabel || sources.NullOrEmpty() || sources.Count > 1 || parent.stackCount > 1)
			{
				return label;
			}
			return "ThingOfSource".Translate(label, sources[0]);
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			otherStack.TryGetComp<CompHasSources>()?.TransferSourcesTo(this, count);
		}

		public override void PostSplitOff(Thing piece)
		{
			CompHasSources compHasSources = piece.TryGetComp<CompHasSources>();
			if (compHasSources != null && piece != parent)
			{
				compHasSources.sources.Clear();
				TransferSourcesTo(compHasSources, piece.stackCount);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (sources.NullOrEmpty())
			{
				return base.CompInspectStringExtra();
			}
			string text = string.Empty;
			if (sources.Count > 1 || !Props.affectLabel)
			{
				tmpSourceLabels.Clear();
				int num = Mathf.Min(sources.Count, 9);
				for (int i = 0; i < num; i++)
				{
					tmpSourceLabels.Add(sources[i]);
				}
				if (sources.Count > 9)
				{
					tmpSourceLabels.Add("Etc".Translate() + "...");
				}
				string text2 = (Props.inspectStringLabel ?? ((string)"ThingOf".Translate(Find.ActiveLanguageWorker.Pluralize(parent.LabelNoCount)))).CapitalizeFirst();
				text = text + text2 + ": " + tmpSourceLabels.ToCommaList();
			}
			return text;
		}

		public override void PostExposeData()
		{
			Scribe_Collections.Look(ref sources, "sources", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && sources == null)
			{
				sources = new List<string>();
			}
		}
	}
}
