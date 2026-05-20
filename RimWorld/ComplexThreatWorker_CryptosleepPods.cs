using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ComplexThreatWorker_CryptosleepPods : ComplexThreatWorker
{
	private const string TriggerOpenAction = "TriggerOpenAction";

	private const string CompletedOpenAction = "CompletedOpenAction";

	private const float RoomEntryTriggerChance = 0.25f;

	protected override bool CanResolveInt(ComplexResolveParams parms)
	{
		if (base.CanResolveInt(parms) && ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientCryptosleepPod, parms.room, parms.map, out var _) && parms.points >= PawnKindDefOf.AncientSoldier.combatPower)
		{
			if (parms.hostileFaction != null)
			{
				return parms.hostileFaction == Faction.OfAncientsHostile;
			}
			return true;
		}
		return false;
	}

	protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
	{
		List<Thing> list = SpawnCasketsWithHostiles(parms.room, parms.points, parms.triggerSignal, parms.map);
		SignalAction_OpenCasket signalAction_OpenCasket = (SignalAction_OpenCasket)ThingMaker.MakeThing(ThingDefOf.SignalAction_OpenCasket);
		signalAction_OpenCasket.signalTag = parms.triggerSignal;
		signalAction_OpenCasket.caskets.AddRange(list);
		signalAction_OpenCasket.completedSignalTag = "CompletedOpenAction" + Find.UniqueIDsManager.GetNextSignalTagID();
		if (parms.delayTicks.HasValue)
		{
			signalAction_OpenCasket.delayTicks = parms.delayTicks.Value;
			SignalAction_Message obj = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
			obj.signalTag = parms.triggerSignal;
			obj.lookTargets = list;
			obj.messageType = MessageTypeDefOf.ThreatBig;
			obj.message = "MessageSleepingThreatDelayActivated".Translate(Faction.OfAncientsHostile, signalAction_OpenCasket.delayTicks.ToStringTicksToPeriod());
			GenSpawn.Spawn(obj, parms.room.rects[0].CenterCell, parms.map);
		}
		GenSpawn.Spawn(signalAction_OpenCasket, parms.map.Center, parms.map);
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i] is Building_Casket building_Casket))
			{
				continue;
			}
			foreach (Thing item in (IEnumerable<Thing>)building_Casket.GetDirectlyHeldThings())
			{
				if (item is Pawn pawn)
				{
					threatPointsUsed += pawn.kindDef.combatPower;
				}
			}
		}
		SignalAction_Message obj2 = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
		obj2.signalTag = signalAction_OpenCasket.completedSignalTag;
		obj2.lookTargets = list;
		obj2.messageType = MessageTypeDefOf.ThreatBig;
		obj2.message = "MessageSleepingPawnsWokenUp".Translate(Faction.OfAncientsHostile.def.pawnsPlural.CapitalizeFirst());
		GenSpawn.Spawn(obj2, parms.room.rects[0].CenterCell, parms.map);
	}

	private List<Thing> SpawnCasketsWithHostiles(LayoutRoom room, float threatPoints, string openSignal, Map map)
	{
		int num = Mathf.FloorToInt(threatPoints / PawnKindDefOf.AncientSoldier.combatPower);
		List<Thing> list = new List<Thing>();
		for (int i = 0; i < num; i++)
		{
			if (!ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientCryptosleepPod, room, map, out var spawnPosition))
			{
				break;
			}
			Building_AncientCryptosleepPod building_AncientCryptosleepPod = (Building_AncientCryptosleepPod)GenSpawn.Spawn(ThingDefOf.AncientCryptosleepPod, spawnPosition, map);
			building_AncientCryptosleepPod.groupID = Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
			building_AncientCryptosleepPod.openedSignal = openSignal;
			ThingSetMakerParams parms = new ThingSetMakerParams
			{
				podContentsType = PodContentsType.AncientHostile
			};
			List<Thing> list2 = ThingSetMakerDefOf.MapGen_AncientPodContents.root.Generate(parms);
			for (int j = 0; j < list2.Count; j++)
			{
				Pawn pawn = list2[j] as Pawn;
				if (!building_AncientCryptosleepPod.TryAcceptThing(list2[j], allowSpecialEffects: false))
				{
					if (pawn != null)
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					else
					{
						list2[i].Destroy();
					}
				}
			}
			list.Add(building_AncientCryptosleepPod);
		}
		return list;
	}
}
