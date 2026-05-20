using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_DamageUntilDowned : QuestPart
{
	public List<Pawn> pawns = new List<Pawn>();

	public string inSignal;

	public bool allowBleedingWounds = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			for (int i = 0; i < pawns.Count; i++)
			{
				yield return pawns[i];
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (!pawns[i].DestroyedOrNull())
			{
				HealthUtility.DamageUntilDowned(pawns[i], allowBleedingWounds);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref allowBleedingWounds, "allowBleedingWounds", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		pawns.Add(PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
