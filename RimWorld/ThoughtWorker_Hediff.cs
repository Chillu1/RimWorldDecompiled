using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Hediff : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(def.hediff);
			if (firstHediffOfDef == null || firstHediffOfDef.def.stages == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(Mathf.Min(firstHediffOfDef.CurStageIndex, firstHediffOfDef.def.stages.Count - 1, def.stages.Count - 1));
		}
	}
}
