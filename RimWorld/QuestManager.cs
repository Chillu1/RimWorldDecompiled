using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestManager : IExposable
{
	private List<Quest> historicalQuests = new List<Quest>();

	private List<Quest> activeQuests = new List<Quest>();

	private List<Quest> allQuests = new List<Quest>();

	public List<Quest> questsInDisplayOrder = new List<Quest>();

	private List<QuestPart_SituationalThought> cachedSituationalThoughtQuestParts = new List<QuestPart_SituationalThought>();

	private List<QuestPart_ExtraFaction> cachedExtraFactionQuestParts = new List<QuestPart_ExtraFaction>();

	public List<Quest> QuestsListForReading => allQuests;

	public List<Quest> ActiveQuestsListForReading => activeQuests;

	public List<QuestPart_SituationalThought> SituationalThoughtQuestParts => cachedSituationalThoughtQuestParts;

	public List<QuestPart_ExtraFaction> ExtraFactionQuestParts => cachedExtraFactionQuestParts;

	public void Add(Quest quest)
	{
		if (quest == null)
		{
			Log.Error("Tried to add a null quest.");
			return;
		}
		if (Contains(quest))
		{
			Log.Error("Tried to add the same quest twice: " + quest.ToStringSafe());
			return;
		}
		activeQuests.Add(quest);
		allQuests.Add(quest);
		AddToCache(quest);
		Find.SignalManager.RegisterReceiver(quest);
		List<QuestPart> partsListForReading = quest.PartsListForReading;
		for (int i = 0; i < partsListForReading.Count; i++)
		{
			partsListForReading[i].PostQuestAdded();
		}
		quest.PostAdded();
		if (quest.initiallyAccepted)
		{
			quest.Initiate();
		}
	}

	public void Remove(Quest quest)
	{
		if (!Contains(quest))
		{
			Log.Error("Tried to remove non-existent quest: " + quest.ToStringSafe());
			return;
		}
		allQuests.Remove(quest);
		activeQuests.Remove(quest);
		historicalQuests.Remove(quest);
		RemoveFromCache(quest);
		Find.SignalManager.DeregisterReceiver(quest);
	}

	public bool Contains(Quest quest)
	{
		return allQuests.Contains(quest);
	}

	public void QuestManagerTick()
	{
		for (int i = 0; i < historicalQuests.Count; i++)
		{
			historicalQuests[i].QuestTick();
		}
		for (int j = 0; j < activeQuests.Count; j++)
		{
			activeQuests[j].QuestTick();
			if (activeQuests[j].Historical)
			{
				historicalQuests.Add(activeQuests[j]);
				activeQuests[j] = null;
			}
		}
		activeQuests.RemoveAll((Quest quest) => quest == null);
	}

	public bool IsReservedByAnyQuest(Pawn p)
	{
		int count = activeQuests.Count;
		for (int i = 0; i < count; i++)
		{
			if (activeQuests[i] != null && activeQuests[i].QuestReserves(p))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsReservedByAnyQuest(Faction f)
	{
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i] != null && activeQuests[i].QuestReserves(f))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsReservedByAnyQuest(TransportShip ship)
	{
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i] != null && activeQuests[i].QuestReserves(ship))
			{
				return true;
			}
		}
		return false;
	}

	private void AddToCache(Quest quest)
	{
		questsInDisplayOrder.Add(quest);
		questsInDisplayOrder.SortBy((Quest x) => x.TicksSinceAppeared);
		for (int num = 0; num < quest.PartsListForReading.Count; num++)
		{
			if (quest.PartsListForReading[num] is QuestPart_SituationalThought item)
			{
				cachedSituationalThoughtQuestParts.Add(item);
			}
			if (quest.PartsListForReading[num] is QuestPart_ExtraFaction item2)
			{
				cachedExtraFactionQuestParts.Add(item2);
			}
		}
	}

	private void RemoveFromCache(Quest quest)
	{
		questsInDisplayOrder.Remove(quest);
		for (int i = 0; i < quest.PartsListForReading.Count; i++)
		{
			if (quest.PartsListForReading[i] is QuestPart_SituationalThought item)
			{
				cachedSituationalThoughtQuestParts.Remove(item);
			}
			if (quest.PartsListForReading[i] is QuestPart_ExtraFaction item2)
			{
				cachedExtraFactionQuestParts.Remove(item2);
			}
		}
	}

	public void Notify_PawnDiscarded(Pawn pawn)
	{
		for (int i = 0; i < allQuests.Count; i++)
		{
			allQuests[i].Notify_PawnDiscarded(pawn);
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref allQuests, "quests", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			int num = allQuests.RemoveAll((Quest x) => x == null);
			if (num != 0)
			{
				Log.Error(num + " quest(s) were null after loading.");
			}
			int num2 = allQuests.RemoveAll((Quest q) => q.root == null);
			if (num2 != 0)
			{
				Log.Error(num2 + " quest(s) had null roots after loading.");
			}
			cachedExtraFactionQuestParts.Clear();
			cachedSituationalThoughtQuestParts.Clear();
			questsInDisplayOrder.Clear();
			for (int num3 = 0; num3 < allQuests.Count; num3++)
			{
				AddToCache(allQuests[num3]);
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			for (int num4 = 0; num4 < allQuests.Count; num4++)
			{
				Find.SignalManager.RegisterReceiver(allQuests[num4]);
			}
			activeQuests.Clear();
			historicalQuests.Clear();
			for (int num5 = 0; num5 < allQuests.Count; num5++)
			{
				if (allQuests[num5].Historical)
				{
					historicalQuests.Add(allQuests[num5]);
				}
				else
				{
					activeQuests.Add(allQuests[num5]);
				}
			}
		}
		BackCompatibility.PostExposeData(this);
	}

	public void Notify_ThingsProduced(Pawn worker, List<Thing> things)
	{
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i].State == QuestState.Ongoing)
			{
				activeQuests[i].Notify_ThingsProduced(worker, things);
			}
		}
	}

	public void Notify_PlantHarvested(Pawn worker, Thing harvested)
	{
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i].State == QuestState.Ongoing)
			{
				activeQuests[i].Notify_PlantHarvested(worker, harvested);
			}
		}
	}

	public void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i].State == QuestState.Ongoing)
			{
				activeQuests[i].Notify_PawnKilled(pawn, dinfo);
			}
		}
	}

	public void Notify_PawnBorn(Thing baby, Thing birther, Pawn mother, Pawn father)
	{
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i].State == QuestState.Ongoing)
			{
				activeQuests[i].Notify_PawnBorn(baby, birther, mother, father);
			}
		}
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		for (int i = 0; i < allQuests.Count; i++)
		{
			allQuests[i].Notify_FactionRemoved(faction);
		}
	}
}
