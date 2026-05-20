using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_AnyEmptyGrave : RitualObligationTargetFilter
{
	public RitualObligationTargetWorker_AnyEmptyGrave()
	{
	}

	public RitualObligationTargetWorker_AnyEmptyGrave(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		Thing thing = map.listerThings.ThingsInGroup(ThingRequestGroup.Grave).FirstOrDefault((Thing t) => ((Building_Grave)t).Corpse == null);
		if (thing != null)
		{
			yield return thing;
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		return target.HasThing && target.Thing is Building_Grave building_Grave && building_Grave.Corpse == null;
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
			yield return "RitualTargetEmptyGraveInfoAbstract".Translate(parent.ideo.Named("IDEO"));
			yield break;
		}
		Pawn arg = (Pawn)obligation.targetA.Thing;
		yield return "RitualTargetEmptyGraveInfo".Translate(arg.Named("PAWN"));
	}

	public override string LabelExtraPart(RitualObligation obligation)
	{
		return ((Pawn)obligation.targetA.Thing).LabelShort;
	}
}
