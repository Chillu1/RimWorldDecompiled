using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_ShamblerAssault : LordToil
{
	private const int UpdateIntervalTicks = 300;

	public override bool AssignsDuties => false;

	public override bool AllowAggressiveTargetingOfRoamers => true;

	public override void UpdateAllDuties()
	{
		IEnumerable<Pawn> source = from pt in lord.Map.mapPawns.AllHumanlikeSpawned.Concat(lord.Map.mapPawns.SpawnedColonyAnimals).Concat(lord.Map.mapPawns.SpawnedColonyMechs)
			where pt.Faction != Faction.OfEntities && !pt.ThreatDisabled(null)
			select pt;
		if (!source.Any())
		{
			base.Map.lordManager.RemoveLord(lord);
			return;
		}
		for (int num = 0; num < lord.ownedPawns.Count; num++)
		{
			Pawn pawn = lord.ownedPawns[num];
			if (pawn.mindState.enemyTarget == null)
			{
				(pawn.mutant.Hediff as Hediff_Shambler)?.Notify_DelayedAlert(source.RandomElement());
				pawn.mindState.Notify_EngagedTarget();
			}
		}
	}

	public override void LordToilTick()
	{
		if (lord.ticksInToil % 300 == 0)
		{
			UpdateAllDuties();
		}
	}

	public override bool CanAddPawn(Pawn p)
	{
		return p.IsShambler;
	}
}
