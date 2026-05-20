using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_RemoveMemoryThought : QuestPart
{
	public string inSignal;

	public ThoughtDef def;

	public Pawn pawn;

	public Pawn otherPawn;

	public int? count;

	public bool addToLookTargets = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (pawn != null && addToLookTargets)
			{
				yield return pawn;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal) || pawn == null || pawn.needs == null)
		{
			return;
		}
		if (count.HasValue)
		{
			for (int i = 0; i < count.Value; i++)
			{
				Thought_Memory thought_Memory = pawn.needs.mood.thoughts.memories.Memories.FirstOrDefault((Thought_Memory m) => def == m.def && (otherPawn == null || m.otherPawn == otherPawn));
				if (thought_Memory != null)
				{
					pawn.needs.mood.thoughts.memories.RemoveMemory(thought_Memory);
					continue;
				}
				break;
			}
		}
		else if (otherPawn == null)
		{
			pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(def);
		}
		else
		{
			pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(def, otherPawn);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref addToLookTargets, "addToLookTargets", defaultValue: false);
		Scribe_Values.Look(ref count, "count");
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_References.Look(ref otherPawn, "otherPawn");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		def = ThoughtDefOf.DecreeMet ?? ThoughtDefOf.DebugGood;
		pawn = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault();
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (pawn == replace)
		{
			pawn = with;
		}
		if (otherPawn == replace)
		{
			otherPawn = with;
		}
	}
}
