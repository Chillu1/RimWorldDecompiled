using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_IsPrisoner : QuestPartActivable
{
	public Pawn pawn;

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

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (pawn != null && pawn.IsPrisoner)
		{
			Complete(pawn.Named("SUBJECT"));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		if (Find.AnyPlayerHomeMap != null)
		{
			pawn = Find.RandomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault();
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
