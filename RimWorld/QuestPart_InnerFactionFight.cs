using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_InnerFactionFight : QuestPartActivable
{
	public Faction firstFaction;

	public FactionDef secondFactionDef;

	public Faction secondFaction;

	public IntVec3 meetingPos;

	public MapParent mapParent;

	public List<Pawn> pawns = new List<Pawn>();

	public List<string> inSignals = new List<string>();

	public string outSignalEnabled;

	public string outSignalComplete;

	public string factionBecameHostileSignal;

	private bool OneSideDefeated
	{
		get
		{
			if (secondFaction == null)
			{
				return false;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].Destroyed && !pawns[i].Downed)
				{
					if (pawns[i].Faction == firstFaction)
					{
						num++;
					}
					else if (pawns[i].Faction == secondFaction)
					{
						num2++;
					}
				}
			}
			if (num != 0)
			{
				return num2 == 0;
			}
			return true;
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		if (!OneSideDefeated)
		{
			List<FactionRelation> list = new List<FactionRelation>();
			foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
			{
				if (item == firstFaction)
				{
					list.Add(new FactionRelation(item, FactionRelationKind.Hostile));
				}
				else if (!item.def.PermanentlyHostileTo(firstFaction.def))
				{
					list.Add(new FactionRelation(item, FactionRelationKind.Neutral));
				}
			}
			secondFaction = FactionGenerator.NewGeneratedFactionWithRelations(secondFactionDef, list, hidden: true);
			secondFaction.temporary = true;
			Find.FactionManager.Add(secondFaction);
			List<Thing> list2 = new List<Thing>();
			List<Thing> list3 = new List<Thing>();
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].Destroyed)
				{
					if (i % 2 == 0)
					{
						pawns[i].SetFaction(secondFaction);
						list3.Add(pawns[i]);
					}
					else
					{
						list2.Add(pawns[i]);
						pawns[i].GetLord()?.RemovePawn(pawns[i]);
					}
				}
			}
			Lord lord = LordMaker.MakeNewLord(firstFaction, new LordJob_AssaultThings(firstFaction, list3), mapParent.Map);
			foreach (Thing item2 in list2)
			{
				lord.AddPawn((Pawn)item2);
			}
			lord.inSignalLeave = outSignalComplete;
			Lord lord2 = LordMaker.MakeNewLord(secondFaction, new LordJob_AssaultThings(secondFaction, list2), mapParent.Map);
			foreach (Thing item3 in list3)
			{
				lord2.AddPawn((Pawn)item3);
			}
			lord2.inSignalLeave = outSignalComplete;
			if (!outSignalEnabled.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(outSignalEnabled, receivedArgs));
			}
		}
		else
		{
			Complete();
		}
	}

	public override void QuestPartTick()
	{
		if (OneSideDefeated)
		{
			Find.SignalManager.SendSignal(new Signal(outSignalComplete));
			Complete();
		}
	}

	public override bool QuestPartReserves(Faction f)
	{
		if (f != firstFaction)
		{
			if (secondFaction != null)
			{
				return f == secondFaction;
			}
			return false;
		}
		return true;
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (firstFaction == f)
		{
			firstFaction = null;
		}
		else if (secondFaction == f)
		{
			secondFaction = null;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == factionBecameHostileSignal && secondFaction != null)
		{
			secondFaction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile, canSendHostilityLetter: false);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref firstFaction, "firstFaction");
		Scribe_Defs.Look(ref secondFactionDef, "secondFactionDef");
		Scribe_References.Look(ref secondFaction, "secondFaction");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
		Scribe_Values.Look(ref outSignalEnabled, "outSignalEnabled");
		Scribe_Values.Look(ref outSignalComplete, "outSignalComplete");
		Scribe_Values.Look(ref meetingPos, "meetingPos");
		Scribe_Values.Look(ref factionBecameHostileSignal, "factionBecameHostileSignal");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
