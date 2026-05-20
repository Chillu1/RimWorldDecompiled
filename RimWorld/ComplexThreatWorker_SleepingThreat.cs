using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class ComplexThreatWorker_SleepingThreat : ComplexThreatWorker
{
	private const string WakeUpSignalTag = "WakeUp";

	private static readonly IntRange RadiusWakeUpDistanceRange = new IntRange(1, 4);

	private static readonly float RadialTriggerChance = 0.2f;

	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	private Faction Faction => Find.FactionManager.FirstFactionOfDef(def.faction);

	protected override bool CanResolveInt(ComplexResolveParams parms)
	{
		if (base.CanResolveInt(parms) && parms.room != null && Faction != null)
		{
			return GetPawnKindsForPoints(parms.points).Any();
		}
		return false;
	}

	protected abstract IEnumerable<PawnKindDef> GetPawnKindsForPoints(float points);

	protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
	{
		List<Pawn> list = SpawnThreatPawns(parms.room, parms.points, parms.map);
		for (int i = 0; i < list.Count; i++)
		{
			threatPointsUsed += list[i].kindDef.combatPower;
		}
		LordJob_SleepThenAssaultColony lordJob = new LordJob_SleepThenAssaultColony(Faction);
		Lord lord = LordMaker.MakeNewLord(Faction, lordJob, parms.map);
		lord.AddPawns(list);
		if (!parms.passive && Rand.Chance(RadialTriggerChance))
		{
			parms.triggerSignal = ComplexUtility.SpawnRadialDistanceTrigger(list, parms.map, RadiusWakeUpDistanceRange.RandomInRange);
		}
		SignalAction_DormancyWakeUp signalAction_DormancyWakeUp = (SignalAction_DormancyWakeUp)ThingMaker.MakeThing(ThingDefOf.SignalAction_DormancyWakeUp);
		signalAction_DormancyWakeUp.lord = lord;
		signalAction_DormancyWakeUp.signalTag = parms.triggerSignal;
		if (parms.delayTicks.HasValue)
		{
			signalAction_DormancyWakeUp.delayTicks = parms.delayTicks.Value;
			SignalAction_Message obj = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
			obj.signalTag = parms.triggerSignal;
			obj.lookTargets = list;
			obj.messageType = MessageTypeDefOf.ThreatBig;
			obj.allMustBeAsleep = true;
			obj.message = "MessageSleepingThreatDelayActivated".Translate(Faction, signalAction_DormancyWakeUp.delayTicks.ToStringTicksToPeriod());
			GenSpawn.Spawn(obj, parms.room.rects[0].CenterCell, parms.map);
		}
		GenSpawn.Spawn(signalAction_DormancyWakeUp, parms.map.Center, parms.map);
	}

	private List<Pawn> SpawnThreatPawns(LayoutRoom room, float threatPoints, Map map)
	{
		List<Pawn> list = new List<Pawn>();
		tmpCells.Clear();
		tmpCells.AddRange(room.rects.SelectMany((CellRect r) => r.Cells));
		tmpCells.Shuffle();
		float num = threatPoints;
		for (int num2 = 0; num2 < tmpCells.Count; num2++)
		{
			if (CanSpawnAt(tmpCells[num2], map))
			{
				IntVec3 loc = tmpCells[num2];
				IEnumerable<PawnKindDef> pawnKindsForPoints = GetPawnKindsForPoints(num);
				if (!pawnKindsForPoints.Any())
				{
					break;
				}
				PawnKindDef pawnKindDef = pawnKindsForPoints.RandomElement();
				Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, Faction);
				GenSpawn.Spawn(pawn, loc, map);
				list.Add(pawn);
				num -= pawnKindDef.combatPower;
			}
		}
		return list;
	}

	private static bool CanSpawnAt(IntVec3 c, Map map)
	{
		if (c.Standable(map) && c.GetFirstPawn(map) == null)
		{
			return c.GetDoor(map) == null;
		}
		return false;
	}
}
