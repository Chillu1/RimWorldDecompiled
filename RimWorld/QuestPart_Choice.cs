using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Choice : QuestPart
	{
		public class Choice : IExposable
		{
			public List<QuestPart> questParts = new List<QuestPart>();

			public List<Reward> rewards = new List<Reward>();

			public void ExposeData()
			{
				Scribe_Collections.Look(ref questParts, "questParts", LookMode.Reference);
				Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
			}
		}

		public string inSignalChoiceUsed;

		public List<Choice> choices = new List<Choice>();

		public bool choiceUsed;

		public override bool PreventsAutoAccept => choices.Count >= 2;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignalChoiceUsed))
			{
				return;
			}
			choiceUsed = true;
			for (int i = 0; i < choices.Count; i++)
			{
				for (int j = 0; j < choices[i].rewards.Count; j++)
				{
					choices[i].rewards[j].Notify_Used();
				}
			}
		}

		public override void Notify_PreCleanup()
		{
			base.Notify_PreCleanup();
			for (int i = 0; i < choices.Count; i++)
			{
				for (int j = 0; j < choices[i].rewards.Count; j++)
				{
					choices[i].rewards[j].Notify_PreCleanup();
				}
			}
		}

		public void Choose(Choice choice)
		{
			for (int num = choices.Count - 1; num >= 0; num--)
			{
				if (choices[num] != choice)
				{
					for (int i = 0; i < choices[num].questParts.Count; i++)
					{
						if (!choice.questParts.Contains(choices[num].questParts[i]))
						{
							choices[num].questParts[i].Notify_PreCleanup();
							choices[num].questParts[i].Cleanup();
							quest.RemovePart(choices[num].questParts[i]);
						}
					}
					choices.RemoveAt(num);
				}
			}
		}

		public override void PreQuestAccept()
		{
			base.PreQuestAccept();
			if (choices.Count >= 2)
			{
				Log.Error("Tried to accept a quest but " + GetType().Name + " still has a choice unresolved. Auto-choosing the first option.");
				Choose(choices[0]);
			}
		}

		public override void PostQuestAdded()
		{
			base.PostQuestAdded();
			for (int i = 0; i < choices.Count; i++)
			{
				for (int j = 0; j < choices[i].rewards.Count; j++)
				{
					Reward_Items reward_Items;
					if ((reward_Items = (choices[i].rewards[j] as Reward_Items)) == null)
					{
						continue;
					}
					for (int k = 0; k < reward_Items.items.Count; k++)
					{
						if (reward_Items.items[k].def == ThingDefOf.PsychicAmplifier)
						{
							Find.History.Notify_PsylinkAvailable();
							return;
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignalChoiceUsed, "inSignalChoiceUsed");
			Scribe_Collections.Look(ref choices, "choices", LookMode.Deep);
			Scribe_Values.Look(ref choiceUsed, "choiceUsed", defaultValue: false);
		}
	}
}
