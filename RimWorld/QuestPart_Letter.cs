using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_Letter : QuestPart
{
	public string inSignal;

	public Letter letter;

	public bool getLookTargetsFromSignal = true;

	public MapParent useColonistsOnMap;

	public string getColonistsFromSignal;

	public bool useColonistsFromCaravanArg;

	public string chosenPawnSignal;

	public string acceptedVisitorsSignal;

	public List<Pawn> visitors;

	public bool filterDeadPawnsFromLookTargets;

	private List<Pawn> colonistsFromSignal = new List<Pawn>();

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			GlobalTargetInfo globalTargetInfo = letter.lookTargets.TryGetPrimaryTarget();
			if (globalTargetInfo.IsValid)
			{
				yield return globalTargetInfo;
			}
		}
	}

	public override IEnumerable<Faction> InvolvedFactions
	{
		get
		{
			foreach (Faction involvedFaction in base.InvolvedFactions)
			{
				yield return involvedFaction;
			}
			if (letter.relatedFaction != null)
			{
				yield return letter.relatedFaction;
			}
		}
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (letter.relatedFaction == faction)
		{
			letter.relatedFaction = null;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!string.IsNullOrEmpty(getColonistsFromSignal) && signal.tag == getColonistsFromSignal)
		{
			if (signal.args.TryGetArg("SUBJECT", out var arg))
			{
				ReadPawns(arg.arg);
			}
			if (signal.args.TryGetArg("SENT", out var arg2))
			{
				ReadPawns(arg2.arg);
			}
		}
		if (!(signal.tag == inSignal))
		{
			return;
		}
		Letter letter = Gen.MemberwiseClone(this.letter);
		letter.ID = Find.UniqueIDsManager.GetNextLetterID();
		if (letter is ChoiceLetter choiceLetter)
		{
			choiceLetter.quest = quest;
		}
		if (letter is ChoiceLetter_ChoosePawn choiceLetter_ChoosePawn)
		{
			if (useColonistsOnMap != null && useColonistsOnMap.HasMap)
			{
				choiceLetter_ChoosePawn.pawns.Clear();
				choiceLetter_ChoosePawn.pawns.AddRange(useColonistsOnMap.Map.mapPawns.FreeColonists);
				choiceLetter_ChoosePawn.chosenPawnSignal = chosenPawnSignal;
			}
			if (useColonistsFromCaravanArg && signal.args.TryGetArg("CARAVAN", out Caravan arg3) && arg3 != null)
			{
				choiceLetter_ChoosePawn.pawns.Clear();
				choiceLetter_ChoosePawn.pawns.AddRange(arg3.PawnsListForReading.Where((Pawn x) => x.IsFreeColonist));
				choiceLetter_ChoosePawn.chosenPawnSignal = chosenPawnSignal;
			}
			if (!string.IsNullOrEmpty(getColonistsFromSignal))
			{
				colonistsFromSignal.RemoveAll((Pawn x) => x.Dead);
				choiceLetter_ChoosePawn.pawns.Clear();
				choiceLetter_ChoosePawn.pawns.AddRange(colonistsFromSignal);
				choiceLetter_ChoosePawn.chosenPawnSignal = chosenPawnSignal;
			}
		}
		if (letter is ChoiceLetter_AcceptVisitors choiceLetter_AcceptVisitors)
		{
			choiceLetter_AcceptVisitors.acceptedSignal = acceptedVisitorsSignal;
			if (visitors != null)
			{
				choiceLetter_AcceptVisitors.pawns.AddRange(visitors);
			}
		}
		if (getLookTargetsFromSignal && !letter.lookTargets.IsValid() && SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out var lookTargets))
		{
			letter.lookTargets = lookTargets;
		}
		letter.Label = signal.args.GetFormattedText(letter.Label);
		ChoiceLetter choiceLetter2 = letter as ChoiceLetter;
		bool flag = true;
		if (choiceLetter2 != null)
		{
			choiceLetter2.title = signal.args.GetFormattedText(choiceLetter2.title);
			choiceLetter2.Text = signal.args.GetFormattedText(choiceLetter2.Text);
			if (choiceLetter2.Text.NullOrEmpty())
			{
				flag = false;
			}
		}
		if (filterDeadPawnsFromLookTargets)
		{
			for (int num = letter.lookTargets.targets.Count - 1; num >= 0; num--)
			{
				Thing thing = letter.lookTargets.targets[num].Thing;
				if (thing is Pawn { Dead: not false })
				{
					letter.lookTargets.targets.Remove(thing);
				}
			}
		}
		if (flag)
		{
			Find.LetterStack.ReceiveLetter(letter);
		}
		void ReadPawns(object obj)
		{
			if (obj is Pawn item && !colonistsFromSignal.Contains(item))
			{
				colonistsFromSignal.Add(item);
			}
			if (obj is List<Pawn> source)
			{
				colonistsFromSignal.AddRange(source.Where((Pawn p) => !colonistsFromSignal.Contains(p)));
			}
			if (obj is List<Thing> source2)
			{
				colonistsFromSignal.AddRange(from Pawn p in source2.Where((Thing t) => t is Pawn)
					where !colonistsFromSignal.Contains(p)
					select p);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Deep.Look(ref letter, "letter");
		Scribe_Values.Look(ref getLookTargetsFromSignal, "getLookTargetsFromSignal", defaultValue: true);
		Scribe_References.Look(ref useColonistsOnMap, "useColonistsOnMap");
		Scribe_Values.Look(ref useColonistsFromCaravanArg, "useColonistsFromCaravanArg", defaultValue: false);
		Scribe_Values.Look(ref chosenPawnSignal, "chosenPawnSignal");
		Scribe_Values.Look(ref filterDeadPawnsFromLookTargets, "filterDeadPawnsFromLookTargets", defaultValue: false);
		Scribe_Values.Look(ref getColonistsFromSignal, "getColonistsFromSignal");
		Scribe_Collections.Look(ref colonistsFromSignal, "colonistsFromSignal", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
		{
			colonistsFromSignal.RemoveAll((Pawn x) => x == null);
		}
		Scribe_Values.Look(ref acceptedVisitorsSignal, "acceptedVisitorsSignal");
		Scribe_Collections.Look(ref visitors, "visitors", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			visitors?.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		letter = LetterMaker.MakeLetter("DEV: Test", "Test text", LetterDefOf.PositiveEvent);
	}
}
