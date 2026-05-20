using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class HediffComp_VerbGiver : HediffComp, IVerbOwner
{
	public VerbTracker verbTracker;

	public HediffCompProperties_VerbGiver Props => (HediffCompProperties_VerbGiver)props;

	public VerbTracker VerbTracker => verbTracker;

	public List<VerbProperties> VerbProperties => Props.verbs;

	public List<Tool> Tools => Props.tools;

	Thing IVerbOwner.ConstantCaster => base.Pawn;

	ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => Props.ownerTypeOverride ?? ImplementOwnerTypeDefOf.Hediff;

	public HediffComp_VerbGiver()
	{
		verbTracker = new VerbTracker(this);
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && verbTracker == null)
		{
			verbTracker = new VerbTracker(this);
		}
	}

	public override void CompPostTick(ref float _)
	{
		verbTracker.VerbsTick();
	}

	string IVerbOwner.UniqueVerbOwnerID()
	{
		return parent.GetUniqueLoadID() + "_" + parent.comps.IndexOf(this);
	}

	bool IVerbOwner.VerbsStillUsableBy(Pawn p)
	{
		return p.health.hediffSet.hediffs.Contains(parent);
	}
}
