using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_GraveWithTarget : RitualObligationTargetFilter
{
	public RitualObligationTargetWorker_GraveWithTarget()
	{
	}

	public RitualObligationTargetWorker_GraveWithTarget(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		Thing thing = map.listerThings.ThingsInGroup(ThingRequestGroup.Grave).FirstOrDefault((Thing t) => ((Building_Grave)t).Corpse == obligation.targetA.Thing);
		if (thing != null)
		{
			yield return thing;
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		return target.HasThing && target.Thing is Building_Grave building_Grave && (building_Grave.Corpse == obligation.targetA.Thing || building_Grave.Corpse?.InnerPawn == obligation.targetA.Thing);
	}

	public override bool ObligationTargetsValid(RitualObligation obligation)
	{
		Corpse corpse = (obligation.targetA.Thing as Pawn)?.Corpse;
		if (obligation.targetA.HasThing)
		{
			if (corpse != null)
			{
				return !corpse.Destroyed;
			}
			return true;
		}
		return false;
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		if (obligation == null)
		{
			if (Find.IdeoManager.classicMode)
			{
				yield return "RitualTargetGraveInfoAbstractNoIdeo".Translate();
			}
			else
			{
				yield return "RitualTargetGraveInfoAbstract".Translate(parent.ideo.Named("IDEO"));
			}
			yield break;
		}
		Pawn pawn = obligation.targetA.Thing as Pawn;
		if (pawn == null)
		{
			pawn = ((Corpse)obligation.targetA.Thing).InnerPawn;
		}
		yield return "RitualTargetGraveInfo".Translate(pawn.Named("PAWN"));
	}

	public override string LabelExtraPart(RitualObligation obligation)
	{
		if (obligation.targetA.Thing is Pawn pawn)
		{
			return pawn.LabelShort;
		}
		return ((Corpse)obligation.targetA.Thing).InnerPawn.LabelShort;
	}
}
