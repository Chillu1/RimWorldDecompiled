using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public abstract class ComplexThreatWorker
{
	public ComplexThreatDef def;

	private const string ThreatTriggerSignal = "ThreatTriggerSignal";

	public void Resolve(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings, StringBuilder debug = null)
	{
		try
		{
			bool flag = false;
			if (parms.triggerSignal.NullOrEmpty())
			{
				if (TryGetThingTriggerSignal(parms, out var triggerSignal))
				{
					parms.triggerSignal = triggerSignal;
					debug?.AppendLine("--> Threat trigger: Existing thing");
				}
				else if (def.fallbackToRoomEnteredTrigger)
				{
					parms.triggerSignal = ComplexUtility.SpawnRoomEnteredTrigger(parms.room, parms.map);
					flag = true;
					debug?.AppendLine("--> Threat trigger: Room entry");
				}
				else
				{
					if (!def.allowPassive)
					{
						Log.Warning("Unable to generate a trigger for threat " + def.defName);
						return;
					}
					parms.triggerSignal = def.defName + Find.UniqueIDsManager.GetNextSignalTagID();
					parms.passive = true;
					debug?.AppendLine("--> Threat trigger: None. Passive threat.");
				}
			}
			float points = parms.points;
			if (!parms.passive && Rand.Chance(def.delayChance))
			{
				int num = def.delayTickOptions.RandomElement();
				parms.points = points;
				parms.delayTicks = num;
				float num2 = def.threatFactorOverDelayTicksCurve.Evaluate(num);
				parms.points *= num2;
				if (debug != null)
				{
					debug.AppendLine($"--> Threat delay ticks: {num}");
					debug.AppendLine($"--> Threat delay points factor: {num2}");
					debug.AppendLine($"--> Threat points post delay factor: {parms.points}");
				}
			}
			if (!parms.passive && !flag && Rand.Chance(def.spawnInOtherRoomChance))
			{
				ComplexResolveParams parms2 = parms;
				foreach (LayoutRoom item in parms.allRooms.InRandomOrder())
				{
					LayoutRoom room = (parms2.room = item);
					if (def.Worker.CanResolve(parms2))
					{
						parms.room = room;
						break;
					}
				}
			}
			float threatPointsUsed2 = 0f;
			ResolveInt(parms, ref threatPointsUsed2, outSpawnedThings);
			if (parms.passive)
			{
				threatPointsUsed2 *= def.postSpawnPassiveThreatFactor;
				debug?.AppendLine($"--> Threat post spawn passive factor: {def.postSpawnPassiveThreatFactor}");
			}
			debug?.AppendLine($"--> Total points used: {threatPointsUsed2}");
			threatPointsUsed += threatPointsUsed2;
		}
		catch (Exception ex)
		{
			Log.Error("Exception resolving " + GetType().Name + ": " + ex);
		}
	}

	private bool TryGetThingTriggerSignal(ComplexResolveParams threatParams, out string triggerSignal)
	{
		if (threatParams.room == null || threatParams.spawnedThings.NullOrEmpty())
		{
			triggerSignal = null;
			return false;
		}
		LayoutRoom room = threatParams.room;
		for (int i = 0; i < threatParams.spawnedThings.Count; i++)
		{
			Thing thing = threatParams.spawnedThings[i];
			if (!room.rects.Any((CellRect r) => r.Contains(thing.Position)))
			{
				continue;
			}
			CompHackable compHackable = thing.TryGetComp<CompHackable>();
			if (compHackable != null && !compHackable.IsHacked)
			{
				if (Rand.Bool)
				{
					if (compHackable.hackingStartedSignal == null)
					{
						compHackable.hackingStartedSignal = "ThreatTriggerSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
					}
					triggerSignal = compHackable.hackingStartedSignal;
				}
				else
				{
					if (compHackable.hackingCompletedSignal == null)
					{
						compHackable.hackingCompletedSignal = "ThreatTriggerSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
					}
					triggerSignal = compHackable.hackingCompletedSignal;
				}
				return true;
			}
			if (thing is Building_Casket { CanOpen: not false } building_Casket)
			{
				if (building_Casket.openedSignal.NullOrEmpty())
				{
					building_Casket.openedSignal = "ThreatTriggerSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
				}
				triggerSignal = building_Casket.openedSignal;
				return true;
			}
		}
		triggerSignal = null;
		return false;
	}

	public bool CanResolve(ComplexResolveParams parms)
	{
		try
		{
			return CanResolveInt(parms);
		}
		catch (Exception ex)
		{
			Log.Error("Exception test running " + GetType().Name + ": " + ex);
			return false;
		}
	}

	protected abstract void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> spawnedThings);

	protected virtual bool CanResolveInt(ComplexResolveParams parms)
	{
		if ((float)def.minPoints <= parms.points && parms.room?.requiredDef == null)
		{
			if (!def.allowPassive && !def.fallbackToRoomEnteredTrigger)
			{
				return !parms.triggerSignal.NullOrEmpty();
			}
			return true;
		}
		return false;
	}
}
