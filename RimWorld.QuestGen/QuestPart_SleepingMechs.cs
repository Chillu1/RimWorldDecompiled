using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld.QuestGen;

public class QuestPart_SleepingMechs : QuestPart
{
	public string inSignal;

	public string inSignalTookDamage;

	public string inSignalLockedOut;

	public Thing defendThing;

	public MapParent mapParent;

	public float points;

	private Lord lord;

	private bool wokeUp;

	private const float HPThresholdWakeUp = 0.75f;

	private static readonly SimpleCurve ThreatPointsToMechPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 150f),
		new CurvePoint(1000f, 500f),
		new CurvePoint(10000f, 2000f)
	};

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal)
		{
			SpawnMechs();
		}
		else
		{
			if (wokeUp)
			{
				return;
			}
			if (signal.tag == inSignalTookDamage)
			{
				if ((float)defendThing.HitPoints < (float)defendThing.MaxHitPoints * 0.75f)
				{
					TryWakeMechsUp();
				}
			}
			else if (signal.tag == inSignalLockedOut)
			{
				TryWakeMechsUp();
			}
		}
	}

	private void TryWakeMechsUp()
	{
		if (wokeUp)
		{
			return;
		}
		wokeUp = true;
		if (lord == null)
		{
			return;
		}
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			ownedPawn.TryGetComp<CompCanBeDormant>().WakeUp();
		}
	}

	private void SpawnMechs()
	{
		Map map = mapParent?.Map;
		if (map == null)
		{
			return;
		}
		float num = ThreatPointsToMechPointsCurve.Evaluate(points);
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			tile = map.Tile,
			faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Mechanoid),
			points = num
		}).ToList();
		if (!list.Any())
		{
			return;
		}
		lord = LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing> { defendThing }, Faction.OfMechanoids, 40f, defendThing.Position, canAssaultColony: false, isMechCluster: false), map);
		foreach (Pawn item in list)
		{
			GenSpawn.Spawn(item, CellFinder.RandomClosewalkCellNear(defendThing.Position, map, 5), map);
			lord.AddPawn(item);
			CompCanBeDormant compCanBeDormant = item.TryGetComp<CompCanBeDormant>();
			compCanBeDormant.ToSleep();
			compCanBeDormant.wakeUpSignalTag = inSignal;
		}
		if (defendThing is Building b)
		{
			lord.AddBuilding(b);
		}
	}

	public override void Cleanup()
	{
		defendThing = null;
		mapParent = null;
		lord = null;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref inSignalTookDamage, "inSignalTookDamage");
		Scribe_Values.Look(ref inSignalLockedOut, "inSignalLockedOut");
		Scribe_References.Look(ref defendThing, "defendThing");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_References.Look(ref lord, "lord");
		Scribe_Values.Look(ref points, "points", 0f);
	}
}
