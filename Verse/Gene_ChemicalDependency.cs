using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class Gene_ChemicalDependency : Gene
{
	public int lastIngestedTick;

	public override bool Active
	{
		get
		{
			if (base.Active && pawn != null)
			{
				return !pawn.IsGhoul;
			}
			return false;
		}
	}

	public Hediff_ChemicalDependency LinkedHediff
	{
		get
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_ChemicalDependency hediff_ChemicalDependency && hediff_ChemicalDependency.chemical == def.chemical)
				{
					return hediff_ChemicalDependency;
				}
			}
			return null;
		}
	}

	public override void PostAdd()
	{
		if (!ModLister.CheckBiotech("Chemical dependency"))
		{
			return;
		}
		base.PostAdd();
		if (def.chemical.addictionHediff != null)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def.chemical.addictionHediff);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
		}
		AddHediff();
		lastIngestedTick = Find.TickManager.TicksGame;
	}

	private void AddHediff()
	{
		if (Active)
		{
			Hediff_ChemicalDependency hediff_ChemicalDependency = (Hediff_ChemicalDependency)HediffMaker.MakeHediff(HediffDefOf.GeneticDrugNeed, pawn);
			hediff_ChemicalDependency.chemical = def.chemical;
			pawn.health.AddHediff(hediff_ChemicalDependency);
		}
	}

	public override void PostRemove()
	{
		Hediff_ChemicalDependency linkedHediff = LinkedHediff;
		if (linkedHediff != null)
		{
			pawn.health.RemoveHediff(linkedHediff);
		}
		base.PostRemove();
	}

	public override void Notify_IngestedThing(Thing thing, int numTaken)
	{
		if (thing.def.thingCategories.NullOrEmpty() || thing.def.thingCategories.Contains(ThingCategoryDefOf.Drugs))
		{
			CompDrug compDrug = thing.TryGetComp<CompDrug>();
			if (compDrug != null && compDrug.Props.chemical == def.chemical)
			{
				Reset();
			}
		}
	}

	public override void Reset()
	{
		Hediff_ChemicalDependency linkedHediff = LinkedHediff;
		if (linkedHediff != null)
		{
			linkedHediff.Severity = linkedHediff.def.initialSeverity;
		}
		else
		{
			AddHediff();
		}
		lastIngestedTick = Find.TickManager.TicksGame;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastIngestedTick, "lastIngestedTick", 0);
	}
}
