using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class Hediff_ChemicalDependency : HediffWithComps
{
	public ChemicalDef chemical;

	[Unsaved(false)]
	private Gene_ChemicalDependency cachedDependencyGene;

	public override string LabelBase => "ChemicalDependency".Translate(chemical.Named("CHEMICAL"));

	public override bool ShouldRemove
	{
		get
		{
			if (LinkedGene != null)
			{
				return !LinkedGene.Active;
			}
			return true;
		}
	}

	public bool ShouldSatify => Severity >= def.stages[2].minSeverity - 0.1f;

	public Gene_ChemicalDependency LinkedGene
	{
		get
		{
			if (cachedDependencyGene == null && pawn.genes != null)
			{
				List<Gene> genesListForReading = pawn.genes.GenesListForReading;
				for (int i = 0; i < genesListForReading.Count; i++)
				{
					if (genesListForReading[i] is Gene_ChemicalDependency gene_ChemicalDependency && gene_ChemicalDependency.def.chemical == chemical)
					{
						cachedDependencyGene = gene_ChemicalDependency;
						break;
					}
				}
			}
			return cachedDependencyGene;
		}
	}

	public override float Severity
	{
		get
		{
			if (LinkedGene == null || !LinkedGene.Active)
			{
				return def.initialSeverity;
			}
			return base.Severity;
		}
		set
		{
			base.Severity = value;
		}
	}

	public override string TipStringExtra
	{
		get
		{
			string text = base.TipStringExtra;
			Gene_ChemicalDependency linkedGene = LinkedGene;
			if (linkedGene != null)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += "GeneDefChemicalNeedDurationDesc".Translate(chemical.label, pawn.Named("PAWN"), "PeriodDays".Translate(5f).Named("DEFICIENCYDURATION"), "PeriodDays".Translate(30f).Named("COMADURATION"), "PeriodDays".Translate(60f).Named("DEATHDURATION")).Resolve();
				text = text + "\n\n" + "LastIngestedDurationAgo".Translate(chemical.Named("CHEMICAL"), (Find.TickManager.TicksGame - linkedGene.lastIngestedTick).ToStringTicksToPeriod().Named("DURATION")).Resolve();
			}
			return text;
		}
	}

	public override bool TryMergeWith(Hediff other)
	{
		if (!(other is Hediff_ChemicalDependency hediff_ChemicalDependency))
		{
			return false;
		}
		if (hediff_ChemicalDependency.chemical == chemical)
		{
			return base.TryMergeWith(other);
		}
		return false;
	}

	public override void CopyFrom(Hediff other)
	{
		base.CopyFrom(other);
		if (other is Hediff_ChemicalDependency hediff_ChemicalDependency)
		{
			chemical = hediff_ChemicalDependency.chemical;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref chemical, "chemical");
	}
}
