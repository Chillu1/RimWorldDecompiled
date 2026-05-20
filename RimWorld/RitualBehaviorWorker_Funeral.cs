using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualBehaviorWorker_Funeral : RitualBehaviorWorker
{
	public RitualBehaviorWorker_Funeral()
	{
	}

	public RitualBehaviorWorker_Funeral(RitualBehaviorDef def)
		: base(def)
	{
	}

	public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
	{
		if (target.HasThing && target.Thing is Building_Grave { Corpse: not null } building_Grave && building_Grave.Corpse.InnerPawn.IsSlave)
		{
			return "CantStartFuneralForSlave".Translate(building_Grave.Corpse.InnerPawn);
		}
		return base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
	}
}
