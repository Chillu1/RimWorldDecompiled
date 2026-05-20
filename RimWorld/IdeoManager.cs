using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IdeoManager : IExposable
{
	private List<Faction> npcWithIdeoTmp = new List<Faction>();

	private List<LordJob_Ritual> activeRitualsTmp = new List<LordJob_Ritual>();

	[Unsaved(false)]
	private Ideo horaxianInt;

	private List<Ideo> ideos = new List<Ideo>();

	private List<Ideo> toRemove = new List<Ideo>();

	public bool classicMode;

	public List<StyleCategoryDef> selectedStyleCategories = new List<StyleCategoryDef>();

	public int ticksToNextGauranlenSpawn = 3600000;

	public int lastPsychicRitualPerformedTick;

	public int lastResettledTick;

	public List<Ideo> IdeosListForReading => ideos;

	public IEnumerable<Ideo> IdeosInViewOrder
	{
		get
		{
			IEnumerable<Faction> factions = Find.FactionManager.AllFactionsInViewOrder;
			return IdeosListForReading.Where((Ideo i) => !i.hidden).OrderBy(delegate(Ideo ideo)
			{
				int num = 0;
				int num2 = int.MaxValue;
				foreach (Faction item in factions)
				{
					if (item.ideos != null && item.ideos.IsPrimary(ideo) && !ideo.hidden)
					{
						if (item.IsPlayer)
						{
							return int.MinValue;
						}
						num2 = Mathf.Min(num2, num);
					}
					num++;
				}
				return num2;
			});
		}
	}

	public Ideo Horaxian
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return null;
			}
			if (horaxianInt == null)
			{
				for (int i = 0; i < ideos.Count; i++)
				{
					if (ideos[i].solid)
					{
						horaxianInt = ideos[i];
						break;
					}
				}
			}
			return horaxianInt;
		}
	}

	public bool Add(Ideo ideo)
	{
		if (ideo == null)
		{
			Log.Error("Tried to add a null ideoligion.");
			return false;
		}
		if (ideos.Contains(ideo))
		{
			Log.Error("Tried to add the same ideoligion twice.");
			return false;
		}
		ideos.Add(ideo);
		return true;
	}

	public bool Remove(Ideo ideo)
	{
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (allFaction.ideos != null && allFaction.ideos.AllIdeos.Contains(ideo))
			{
				Log.Error("Faction " + allFaction.Name + " contains ideo " + ideo.name + " which was removed!");
			}
		}
		if (ideos.Remove(ideo))
		{
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				item.ideo?.Notify_IdeoRemoved(ideo);
			}
			Find.PlayLog.Notify_IdeoRemoved(ideo);
			return true;
		}
		return false;
	}

	public List<Faction> GetFactionsWithIdeo(Ideo ideo, bool onlyPrimary = false, bool onlyNpcFactions = false)
	{
		npcWithIdeoTmp.Clear();
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if ((onlyNpcFactions && allFaction == Faction.OfPlayer) || allFaction.ideos == null)
			{
				continue;
			}
			if (onlyPrimary)
			{
				if (allFaction.ideos.IsPrimary(ideo))
				{
					npcWithIdeoTmp.Add(allFaction);
				}
			}
			else if (allFaction.ideos.Has(ideo))
			{
				npcWithIdeoTmp.Add(allFaction);
			}
		}
		return npcWithIdeoTmp;
	}

	public List<LordJob_Ritual> GetActiveRituals(Map map)
	{
		activeRitualsTmp.Clear();
		if (map == null)
		{
			return activeRitualsTmp;
		}
		List<Lord> lords = map.lordManager.lords;
		for (int i = 0; i < lords.Count; i++)
		{
			if (lords[i].LordJob is LordJob_Ritual item)
			{
				activeRitualsTmp.Add(item);
			}
		}
		return activeRitualsTmp;
	}

	public LordJob_Ritual GetActiveRitualOn(TargetInfo target)
	{
		if (target.HasThing && !target.Thing.Spawned)
		{
			return null;
		}
		foreach (LordJob_Ritual activeRitual in GetActiveRituals(target.Map))
		{
			if (activeRitual.selectedTarget == target)
			{
				return activeRitual;
			}
		}
		return null;
	}

	public void SortIdeos()
	{
		ideos.SortByDescending(delegate(Ideo x)
		{
			if (x == Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo)
			{
				return 999;
			}
			int num = 0;
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].ideos != null && allFactionsListForReading[i].ideos.IsPrimary(x))
				{
					num++;
				}
			}
			return num;
		});
	}

	public void IdeoManagerTick()
	{
		for (int i = 0; i < ideos.Count; i++)
		{
			ideos[i].IdeoTick();
		}
		for (int num = toRemove.Count - 1; num >= 0; num--)
		{
			Ideo ideo = toRemove[num];
			toRemove.RemoveAt(num);
			Remove(ideo);
		}
	}

	public void RemoveUnusedStartingIdeos()
	{
		for (int num = ideos.Count - 1; num >= 0; num--)
		{
			if (CanRemoveIdeo(ideos[num]))
			{
				Remove(ideos[num]);
			}
		}
	}

	public void Notify_PawnKilled(Pawn pawn)
	{
		TryQueueIdeoRemoval(pawn.Ideo);
	}

	public void Notify_PawnLeftMap(Pawn pawn)
	{
		TryQueueIdeoRemoval(pawn.Ideo);
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		if (faction.ideos == null)
		{
			return;
		}
		foreach (Ideo allIdeo in faction.ideos.AllIdeos)
		{
			TryQueueIdeoRemoval(allIdeo);
		}
	}

	private bool TryQueueIdeoRemoval(Ideo ideo)
	{
		if (ideo == null)
		{
			return false;
		}
		if (!CanRemoveIdeo(ideo))
		{
			return false;
		}
		if (!toRemove.Contains(ideo))
		{
			toRemove.Add(ideo);
		}
		return true;
	}

	private bool CanRemoveIdeo(Ideo ideo)
	{
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (allFaction.ideos != null && allFaction.ideos.AllIdeos.Contains(ideo))
			{
				return false;
			}
		}
		foreach (Pawn allMap in PawnsFinder.AllMaps)
		{
			if (allMap.ideo != null && allMap.ideo.Ideo == ideo)
			{
				return false;
			}
		}
		return true;
	}

	public void Notify_GameStarted()
	{
		foreach (Ideo ideo in ideos)
		{
			ideo.Notify_GameStarted();
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref ideos, "ideos", LookMode.Deep);
		Scribe_Collections.Look(ref toRemove, "toRemove", LookMode.Reference);
		Scribe_Values.Look(ref classicMode, "classicMode", defaultValue: false);
		Scribe_Collections.Look(ref selectedStyleCategories, "selectedStyleCategories", LookMode.Def);
		Scribe_Values.Look(ref ticksToNextGauranlenSpawn, "ticksToNextGauranlenSpawn", 3600000);
		Scribe_Values.Look(ref lastPsychicRitualPerformedTick, "lastPsychicRitualPerformedTick", 0);
		Scribe_Values.Look(ref lastResettledTick, "lastResettledTick", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (ideos.RemoveAll((Ideo x) => x == null) != 0)
			{
				Log.Error("Some ideoligions were null after loading.");
			}
			if (toRemove == null)
			{
				toRemove = new List<Ideo>();
			}
			if (classicMode && selectedStyleCategories == null)
			{
				selectedStyleCategories = new List<StyleCategoryDef>();
			}
			BackCompatibility.IdeoManagerPostloadInit();
		}
	}
}
