using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_ChangeNeed : QuestPart
{
	public string inSignal;

	public Pawn pawn;

	public NeedDef need;

	public float offset;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (pawn != null)
			{
				yield return pawn;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignal && pawn != null && pawn.needs != null && pawn.needs.TryGetNeed(this.need, out var need))
		{
			need.CurLevel += offset;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_Defs.Look(ref need, "need");
		Scribe_Values.Look(ref offset, "offset", 0f);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		need = NeedDefOf.Food;
		offset = 0.5f;
		if (Find.AnyPlayerHomeMap != null)
		{
			Find.RandomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault();
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (pawn == replace)
		{
			pawn = with;
		}
	}
}
