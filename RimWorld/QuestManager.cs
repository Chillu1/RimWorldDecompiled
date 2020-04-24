using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestManager : IExposable
	{
		private List<Quest> quests = new List<Quest>();

		public List<Quest> questsInDisplayOrder = new List<Quest>();

		private List<QuestPart_SituationalThought> cachedSituationalThoughtQuestParts = new List<QuestPart_SituationalThought>();

		public List<Quest> QuestsListForReading => quests;

		public List<QuestPart_SituationalThought> SituationalThoughtQuestParts => cachedSituationalThoughtQuestParts;

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
			quests.Add(quest);
			AddToCache(quest);
			Find.SignalManager.RegisterReceiver(quest);
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
			quests.Remove(quest);
			RemoveFromCache(quest);
			Find.SignalManager.DeregisterReceiver(quest);
		}

		public bool Contains(Quest quest)
		{
			return quests.Contains(quest);
		}

		public void QuestManagerTick()
		{
			for (int i = 0; i < quests.Count; i++)
			{
				quests[i].QuestTick();
			}
		}

		public bool IsReservedByAnyQuest(Pawn p)
		{
			for (int i = 0; i < quests.Count; i++)
			{
				if (quests[i].QuestReserves(p))
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
			for (int i = 0; i < quest.PartsListForReading.Count; i++)
			{
				QuestPart_SituationalThought questPart_SituationalThought = quest.PartsListForReading[i] as QuestPart_SituationalThought;
				if (questPart_SituationalThought != null)
				{
					cachedSituationalThoughtQuestParts.Add(questPart_SituationalThought);
				}
			}
		}

		private void RemoveFromCache(Quest quest)
		{
			questsInDisplayOrder.Remove(quest);
			for (int i = 0; i < quest.PartsListForReading.Count; i++)
			{
				QuestPart_SituationalThought questPart_SituationalThought = quest.PartsListForReading[i] as QuestPart_SituationalThought;
				if (questPart_SituationalThought != null)
				{
					cachedSituationalThoughtQuestParts.Remove(questPart_SituationalThought);
				}
			}
		}

		public void Notify_PawnDiscarded(Pawn pawn)
		{
			for (int i = 0; i < quests.Count; i++)
			{
				quests[i].Notify_PawnDiscarded(pawn);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref quests, "quests", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				int num = quests.RemoveAll((Quest x) => x == null);
				if (num != 0)
				{
					Log.Error(num + " quest(s) were null after loading.");
				}
				cachedSituationalThoughtQuestParts.Clear();
				questsInDisplayOrder.Clear();
				for (int i = 0; i < quests.Count; i++)
				{
					AddToCache(quests[i]);
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int j = 0; j < quests.Count; j++)
				{
					Find.SignalManager.RegisterReceiver(quests[j]);
				}
			}
			BackCompatibility.PostExposeData(this);
		}

		public void Notify_ThingsProduced(Pawn worker, List<Thing> things)
		{
			for (int i = 0; i < quests.Count; i++)
			{
				if (quests[i].State == QuestState.Ongoing)
				{
					quests[i].Notify_ThingsProduced(worker, things);
				}
			}
		}

		public void Notify_PlantHarvested(Pawn worker, Thing harvested)
		{
			for (int i = 0; i < quests.Count; i++)
			{
				if (quests[i].State == QuestState.Ongoing)
				{
					quests[i].Notify_PlantHarvested(worker, harvested);
				}
			}
		}

		public void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
		{
			for (int i = 0; i < quests.Count; i++)
			{
				if (quests[i].State == QuestState.Ongoing)
				{
					quests[i].Notify_PawnKilled(pawn, dinfo);
				}
			}
		}
	}
}
