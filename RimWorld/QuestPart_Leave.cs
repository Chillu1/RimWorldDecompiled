using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_Leave : QuestPart
{
	public string inSignal;

	public List<Pawn> pawns = new List<Pawn>();

	public bool sendStandardLetter = true;

	public bool leaveOnCleanup = true;

	public string inSignalRemovePawn;

	public bool wakeUp;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
			{
				yield return questLookTarget2;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
		{
			pawns.Remove(arg);
		}
		if (signal.tag == inSignal)
		{
			LeaveQuestPartUtility.MakePawnsLeave(pawns, sendStandardLetter, quest, wakeUp);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (leaveOnCleanup)
		{
			LeaveQuestPartUtility.MakePawnsLeave(pawns, sendStandardLetter, quest, wakeUp);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
		Scribe_Values.Look(ref leaveOnCleanup, "leaveOnCleanup", defaultValue: false);
		Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
		Scribe_Values.Look(ref wakeUp, "wakeUp", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
			if (randomPlayerHomeMap.mapPawns.FreeColonistsCount != 0)
			{
				pawns.Add(randomPlayerHomeMap.mapPawns.FreeColonists.First());
			}
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
