using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ComplexThreatWorker_Ambush : ComplexThreatWorker
{
	private const int SpawnAroundDistance = 5;

	protected override bool CanResolveInt(ComplexResolveParams parms)
	{
		if (base.CanResolveInt(parms))
		{
			if (parms.hostileFaction != null)
			{
				if (def.signalActionAmbushType == SignalActionAmbushType.Mechanoids)
				{
					return parms.hostileFaction == Faction.OfMechanoids;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
	{
		SignalAction_Ambush signalAction_Ambush = (SignalAction_Ambush)ThingMaker.MakeThing(ThingDefOf.SignalAction_Ambush);
		signalAction_Ambush.signalTag = parms.triggerSignal;
		signalAction_Ambush.points = parms.points;
		signalAction_Ambush.ambushType = def.signalActionAmbushType;
		signalAction_Ambush.useDropPods = def.useDropPods;
		if (def.spawnAroundComplex)
		{
			signalAction_Ambush.spawnAround = parms.complexRect.ExpandedBy(5);
		}
		GenSpawn.Spawn(signalAction_Ambush, parms.room.rects[0].CenterCell, parms.map);
		threatPointsUsed += parms.points;
	}
}
