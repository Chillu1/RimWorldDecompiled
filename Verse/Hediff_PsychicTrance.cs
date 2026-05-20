using RimWorld;
using Verse.AI.Group;

namespace Verse;

public class Hediff_PsychicTrance : HediffWithComps
{
	public override bool ShouldRemove
	{
		get
		{
			if (base.ShouldRemove)
			{
				return true;
			}
			if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.PerformHateChant)
			{
				return false;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob is LordJob_PsychicRitual)
			{
				return false;
			}
			return true;
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Psychic trance"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}
}
