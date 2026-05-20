using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_FactionGoodwillChange_ShuttleSentThings : QuestPartActivable
{
	public List<string> inSignalsShuttleSent = new List<string>();

	public string inSignalShuttleDestroyed;

	public int changeNotOnShuttle;

	public Faction faction;

	public bool canSendMessage = true;

	public bool canSendHostilityLetter = true;

	public string reason;

	public List<Thing> things = new List<Thing>();

	public HistoryEventDef historyEvent;

	public override IEnumerable<Faction> InvolvedFactions
	{
		get
		{
			foreach (Faction involvedFaction in base.InvolvedFactions)
			{
				yield return involvedFaction;
			}
			if (faction != null)
			{
				yield return faction;
			}
		}
	}

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			foreach (Thing thing in things)
			{
				yield return thing;
			}
		}
	}

	protected override void ProcessQuestSignal(Signal signal)
	{
		base.ProcessQuestSignal(signal);
		if (inSignalsShuttleSent.Contains(signal.tag) && signal.args.TryGetArg("SENT", out List<Thing> arg))
		{
			int num = 0;
			for (int i = 0; i < things.Count; i++)
			{
				if (!arg.Contains(things[i]))
				{
					num++;
				}
			}
			TryAffectGoodwill(num * changeNotOnShuttle);
			Complete();
		}
		if (signal.tag == inSignalShuttleDestroyed)
		{
			TryAffectGoodwill(things.Count * changeNotOnShuttle);
			Complete();
		}
	}

	public override void Cleanup()
	{
		if (base.State == QuestPartState.Enabled)
		{
			TryAffectGoodwill(things.Count * changeNotOnShuttle);
		}
	}

	private void TryAffectGoodwill(int goodwillChange)
	{
		if (goodwillChange != 0)
		{
			Faction.OfPlayer.TryAffectGoodwillWith(faction, goodwillChange, canSendMessage, canSendHostilityLetter, historyEvent);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref inSignalsShuttleSent, "inSignalsShuttleSent", LookMode.Value);
		Scribe_Values.Look(ref changeNotOnShuttle, "changeNotOnShuttle", 0);
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref canSendMessage, "canSendMessage", defaultValue: true);
		Scribe_Values.Look(ref canSendHostilityLetter, "canSendHostilityLetter", defaultValue: true);
		Scribe_Values.Look(ref reason, "reason");
		Scribe_Collections.Look(ref things, "things", LookMode.Reference);
		Scribe_Values.Look(ref inSignalShuttleDestroyed, "inSignalShuttleDestroyed");
		Scribe_Defs.Look(ref historyEvent, "historyEvent");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			things.RemoveAll((Thing x) => x == null);
		}
	}
}
