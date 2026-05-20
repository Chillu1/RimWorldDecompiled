using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Hediff : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(def.hediff);
		if (firstHediffOfDef?.def.stages == null)
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveAtStage(Mathf.Min(firstHediffOfDef.CurStageIndex, firstHediffOfDef.def.stages.Count - 1, def.stages.Count - 1));
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		string text = base.PostProcessDescription(p, description);
		Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(def.hediff);
		if (firstHediffOfDef == null || !firstHediffOfDef.Visible)
		{
			return text;
		}
		return text + "\n\n" + "CausedBy".Translate() + ": " + firstHediffOfDef.LabelBase.CapitalizeFirst();
	}
}
