using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class QuestPart_DelayedRewardDropPods : QuestPart_AddQuest
{
	public List<Thing> rewards = new List<Thing>();

	public int delayTicks;

	public string customLetterLabel;

	public string customLetterText;

	public Faction faction;

	public Pawn giver;

	public float chance = 1f;

	private List<Thing> rewardThings = new List<Thing>();

	private List<Pawn> rewardPawns = new List<Pawn>();

	public override QuestScriptDef QuestDef => QuestScriptDefOf.DelayedRewardDropPods;

	public override bool CanAdd => Rand.Chance(chance);

	public override Slate GetSlate()
	{
		Slate slate = new Slate();
		slate.Set("rewards", rewards);
		slate.Set("faction", faction);
		slate.Set("giver", giver);
		slate.Set("delayTicks", delayTicks);
		slate.Set("customLetterLabel", customLetterLabel);
		slate.Set("customLetterText", customLetterText);
		return slate;
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (faction == f)
		{
			faction = null;
		}
	}

	public override void PostAdd()
	{
		rewards.Clear();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			rewardThings.Clear();
			rewardPawns.Clear();
			foreach (Thing reward in rewards)
			{
				if (reward is Pawn item)
				{
					rewardPawns.Add(item);
				}
				else
				{
					rewardThings.Add(reward);
				}
			}
		}
		Scribe_References.Look(ref faction, "faction");
		Scribe_References.Look(ref giver, "giver");
		Scribe_Values.Look(ref delayTicks, "delayTicks", 0);
		Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
		Scribe_Values.Look(ref customLetterText, "customLetterText");
		Scribe_Values.Look(ref chance, "chance", 0f);
		Scribe_Collections.Look(ref rewardPawns, "rewardPawns", LookMode.Reference);
		Scribe_Collections.Look(ref rewardThings, "rewardThings", LookMode.Deep);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		rewardPawns.RemoveAll((Pawn x) => x == null);
		foreach (Pawn rewardPawn in rewardPawns)
		{
			rewards.Add(rewardPawn);
		}
		foreach (Thing rewardThing in rewardThings)
		{
			rewards.Add(rewardThing);
		}
		rewardThings.Clear();
		rewardPawns.Clear();
	}
}
