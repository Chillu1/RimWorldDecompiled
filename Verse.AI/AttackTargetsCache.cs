using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class AttackTargetsCache
{
	private Map map;

	private HashSet<IAttackTarget> allTargets = new HashSet<IAttackTarget>();

	private Dictionary<Faction, HashSet<IAttackTarget>> targetsHostileToFaction = new Dictionary<Faction, HashSet<IAttackTarget>>();

	private HashSet<Pawn> pawnsInAggroMentalState = new HashSet<Pawn>();

	private HashSet<Pawn> factionlessHumanlikes = new HashSet<Pawn>();

	private static List<IAttackTarget> targets = new List<IAttackTarget>();

	private static readonly HashSet<IAttackTarget> emptySet = new HashSet<IAttackTarget>();

	private static readonly List<IAttackTarget> tmpTargets = new List<IAttackTarget>();

	private static readonly List<IAttackTarget> tmpToUpdate = new List<IAttackTarget>();

	public HashSet<IAttackTarget> TargetsHostileToColony => TargetsHostileToFaction(Faction.OfPlayer);

	public AttackTargetsCache(Map map)
	{
		this.map = map;
		map.events.FactionAdded += Notify_FactionAdded;
		map.events.FactionRemoved += Notify_FactionRemoved;
	}

	public static void AttackTargetsCacheStaticUpdate()
	{
		targets.Clear();
	}

	public void UpdateTarget(IAttackTarget t)
	{
		if (allTargets.Contains(t))
		{
			DeregisterTarget(t);
			Thing thing = t.Thing;
			if (thing.Spawned && thing.Map == map)
			{
				RegisterTarget(t);
			}
		}
	}

	public List<IAttackTarget> GetPotentialTargetsFor(IAttackTargetSearcher th)
	{
		Thing thing = th.Thing;
		targets.Clear();
		Faction faction = thing.Faction;
		if (faction != null)
		{
			foreach (IAttackTarget item in TargetsHostileToFaction(faction))
			{
				if (thing.HostileTo(item.Thing))
				{
					targets.Add(item);
				}
			}
		}
		foreach (Pawn item2 in pawnsInAggroMentalState)
		{
			if (thing.HostileTo(item2))
			{
				targets.Add(item2);
			}
		}
		foreach (Pawn factionlessHumanlike in factionlessHumanlikes)
		{
			if (thing.HostileTo(factionlessHumanlike))
			{
				targets.Add(factionlessHumanlike);
			}
		}
		Pawn pawn = th as Pawn;
		if (pawn != null && PrisonBreakUtility.IsPrisonBreaking(pawn))
		{
			Faction hostFaction = pawn.guest.HostFaction;
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(hostFaction);
			for (int i = 0; i < list.Count; i++)
			{
				if (thing.HostileTo(list[i]))
				{
					targets.Add(list[i]);
				}
			}
		}
		if (pawn != null && ModsConfig.IdeologyActive && SlaveRebellionUtility.IsRebelling(pawn))
		{
			Faction faction2 = pawn.Faction;
			List<Pawn> list2 = map.mapPawns.SpawnedPawnsInFaction(faction2);
			for (int j = 0; j < list2.Count; j++)
			{
				if (thing.HostileTo(list2[j]))
				{
					targets.Add(list2[j]);
				}
			}
		}
		return targets;
	}

	public HashSet<IAttackTarget> TargetsHostileToFaction(Faction f)
	{
		if (f == null)
		{
			Log.Warning("Called TargetsHostileToFaction with null faction.");
			return emptySet;
		}
		if (targetsHostileToFaction.TryGetValue(f, out var value))
		{
			return value;
		}
		return emptySet;
	}

	public void Notify_ThingSpawned(Thing th)
	{
		if (th is IAttackTarget target)
		{
			RegisterTarget(target);
		}
	}

	public void Notify_ThingDespawned(Thing th)
	{
		if (th is IAttackTarget target)
		{
			DeregisterTarget(target);
		}
	}

	public void Notify_FactionHostilityChanged(Faction f1, Faction f2)
	{
		tmpTargets.Clear();
		foreach (IAttackTarget allTarget in allTargets)
		{
			Thing thing = allTarget.Thing;
			Pawn pawn = thing as Pawn;
			if (thing.Faction == f1 || thing.Faction == f2 || (pawn != null && pawn.HostFaction == f1) || (pawn != null && pawn.HostFaction == f2))
			{
				tmpTargets.Add(allTarget);
			}
		}
		for (int i = 0; i < tmpTargets.Count; i++)
		{
			UpdateTarget(tmpTargets[i]);
		}
		tmpTargets.Clear();
	}

	private void RegisterTarget(IAttackTarget target)
	{
		if (allTargets.Contains(target))
		{
			Log.Warning("Tried to register the same target twice " + target.ToStringSafe() + " in " + GetType());
			return;
		}
		Thing thing = target.Thing;
		if (!thing.Spawned)
		{
			Log.Warning("Tried to register unspawned thing " + thing.ToStringSafe() + " in " + GetType());
			return;
		}
		if (thing.Map != map)
		{
			Log.Warning("Tried to register attack target " + thing.ToStringSafe() + " but its Map is not this one.");
			return;
		}
		allTargets.Add(target);
		List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
		for (int i = 0; i < allFactionsListForReading.Count; i++)
		{
			if (thing.HostileTo(allFactionsListForReading[i]))
			{
				if (!targetsHostileToFaction.ContainsKey(allFactionsListForReading[i]))
				{
					targetsHostileToFaction.Add(allFactionsListForReading[i], new HashSet<IAttackTarget>());
				}
				targetsHostileToFaction[allFactionsListForReading[i]].Add(target);
			}
		}
		if (target is Pawn pawn)
		{
			if (pawn.InAggroMentalState)
			{
				pawnsInAggroMentalState.Add(pawn);
			}
			if (pawn.Faction == null && pawn.RaceProps.Humanlike)
			{
				factionlessHumanlikes.Add(pawn);
			}
		}
	}

	private void DeregisterTarget(IAttackTarget target)
	{
		if (!allTargets.Contains(target))
		{
			Log.Warning("Tried to deregister " + target?.ToString() + " but it's not in " + GetType());
			return;
		}
		allTargets.Remove(target);
		foreach (KeyValuePair<Faction, HashSet<IAttackTarget>> item2 in targetsHostileToFaction)
		{
			item2.Value.Remove(target);
		}
		if (target is Pawn item)
		{
			pawnsInAggroMentalState.Remove(item);
			factionlessHumanlikes.Remove(item);
		}
	}

	private void Notify_FactionAdded(Faction faction)
	{
		if (!targetsHostileToFaction.ContainsKey(faction))
		{
			targetsHostileToFaction.Add(faction, new HashSet<IAttackTarget>());
		}
	}

	private void Notify_FactionRemoved(Faction faction)
	{
		if (targetsHostileToFaction.ContainsKey(faction))
		{
			targetsHostileToFaction.Remove(faction);
		}
	}

	private void Debug_AssertHostile(Faction f, HashSet<IAttackTarget> targets)
	{
		tmpToUpdate.Clear();
		foreach (IAttackTarget target in targets)
		{
			if (!target.Thing.HostileTo(f))
			{
				tmpToUpdate.Add(target);
				Log.Error("Target " + target.ToStringSafe() + " is not hostile to " + f.ToStringSafe() + " (in " + GetType().Name + ") but it's in the list (forgot to update the target somewhere?). Trying to update the target...");
			}
		}
		for (int i = 0; i < tmpToUpdate.Count; i++)
		{
			UpdateTarget(tmpToUpdate[i]);
		}
		tmpToUpdate.Clear();
	}

	public bool Debug_CheckIfInAllTargets(IAttackTarget t)
	{
		if (t != null)
		{
			return allTargets.Contains(t);
		}
		return false;
	}

	public bool Debug_CheckIfHostileToFaction(Faction f, IAttackTarget t)
	{
		if (f == null)
		{
			return false;
		}
		if (t != null)
		{
			return targetsHostileToFaction[f].Contains(t);
		}
		return false;
	}
}
