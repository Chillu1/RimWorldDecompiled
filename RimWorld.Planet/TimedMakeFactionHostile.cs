using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class TimedMakeFactionHostile : WorldObjectComp
{
	private int timerMakeFactionHostile = -1;

	private int ticksLeftMakeFactionHostile = -1;

	private string messageBecameHostile;

	public HistoryEventDef reasonBecameHostile;

	public int? TicksLeft
	{
		get
		{
			if (ticksLeftMakeFactionHostile != -1)
			{
				return ticksLeftMakeFactionHostile;
			}
			return null;
		}
	}

	public void SetupTimer(int ticks, string message = null, HistoryEventDef reason = null)
	{
		timerMakeFactionHostile = ticks;
		ticksLeftMakeFactionHostile = -1;
		messageBecameHostile = message;
		reasonBecameHostile = reason;
	}

	public override void PostMyMapRemoved()
	{
		ticksLeftMakeFactionHostile = -1;
	}

	public override void PostMapGenerate()
	{
		ticksLeftMakeFactionHostile = timerMakeFactionHostile;
	}

	public override void CompTickInterval(int delta)
	{
		if (ticksLeftMakeFactionHostile != -1 && parent.Faction.HostileTo(Faction.OfPlayer))
		{
			ticksLeftMakeFactionHostile = -1;
		}
		if (ticksLeftMakeFactionHostile == -1)
		{
			return;
		}
		ticksLeftMakeFactionHostile = Mathf.Max(ticksLeftMakeFactionHostile - delta, 0);
		if (ticksLeftMakeFactionHostile == 0)
		{
			if (parent.Faction.temporary)
			{
				parent.Faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile);
			}
			else
			{
				parent.Faction.TryAffectGoodwillWith(Faction.OfPlayer, parent.Faction.GoodwillToMakeHostile(Faction.OfPlayer), canSendMessage: true, canSendHostilityLetter: true, reasonBecameHostile);
			}
			if (messageBecameHostile != null)
			{
				Messages.Message(messageBecameHostile, MessageTypeDefOf.NegativeEvent);
			}
			ticksLeftMakeFactionHostile = -1;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref timerMakeFactionHostile, "timerMakeFactionHostile", -1);
		Scribe_Values.Look(ref ticksLeftMakeFactionHostile, "ticksLeftMakeFactionHostile", -1);
		Scribe_Values.Look(ref messageBecameHostile, "messageBecameHostile");
		Scribe_Defs.Look(ref reasonBecameHostile, "reasonBecameHostile");
	}
}
