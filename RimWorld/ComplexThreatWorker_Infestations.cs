using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ComplexThreatWorker_Infestations : ComplexThreatWorker
{
	protected override bool CanResolveInt(ComplexResolveParams parms)
	{
		if (base.CanResolveInt(parms))
		{
			if (parms.hostileFaction != null)
			{
				return parms.hostileFaction == Faction.OfInsects;
			}
			return true;
		}
		return false;
	}

	protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
	{
		float num = Mathf.Max(200f, parms.points);
		int num2 = Mathf.CeilToInt(num / 500f);
		SignalAction_Infestation signalAction_Infestation = (SignalAction_Infestation)ThingMaker.MakeThing(ThingDefOf.SignalAction_Infestation);
		signalAction_Infestation.signalTag = parms.triggerSignal;
		signalAction_Infestation.hivesCount = num2;
		signalAction_Infestation.spawnAnywhereIfNoGoodCell = true;
		signalAction_Infestation.ignoreRoofedRequirement = true;
		signalAction_Infestation.sendStandardLetter = true;
		signalAction_Infestation.insectsPoints = num / (float)num2;
		Map map = parms.map;
		foreach (CellRect item in parms.room.rects.InRandomOrder())
		{
			foreach (IntVec3 item2 in item.Cells.InRandomOrder())
			{
				if (item2.GetThingList(map).Count == 0)
				{
					signalAction_Infestation.overrideLoc = item2;
					break;
				}
			}
		}
		if (parms.delayTicks.HasValue)
		{
			signalAction_Infestation.delayTicks = parms.delayTicks.Value;
			SignalAction_Message signalAction_Message = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
			signalAction_Message.signalTag = signalAction_Infestation.completedSignalTag;
			signalAction_Message.lookTargets = new LookTargets(new GlobalTargetInfo(signalAction_Infestation.overrideLoc.Value, parms.map));
			signalAction_Message.messageType = MessageTypeDefOf.ThreatBig;
			signalAction_Message.message = "MessageInfestationDelayActivated".Translate();
			GenSpawn.Spawn(signalAction_Message, parms.room.rects[0].CenterCell, parms.map);
		}
		GenSpawn.Spawn(signalAction_Infestation, parms.room.rects[0].CenterCell, parms.map);
		threatPointsUsed += num;
	}
}
