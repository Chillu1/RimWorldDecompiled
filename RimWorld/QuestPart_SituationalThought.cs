using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_SituationalThought : QuestPartActivable
{
	public ThoughtDef def;

	public Pawn pawn;

	public int stage;

	public int delayTicks;

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

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_Values.Look(ref stage, "stage", 0);
		Scribe_Values.Look(ref delayTicks, "delayTicks", 0);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		def = ThoughtDefOf.DecreeUnmet;
		pawn = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault();
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (pawn == replace)
		{
			pawn = with;
		}
	}
}
