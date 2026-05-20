using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_ChildInGrowthVat : ThoughtWorker
{
	public override string PostProcessLabel(Pawn p, string label)
	{
		int num = Mathf.RoundToInt(MoodMultiplier(p));
		if (num <= 1)
		{
			return base.PostProcessLabel(p, label);
		}
		return base.PostProcessLabel(p, label) + " x" + num;
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (ChildrenInGrowthVatCount(p) <= 0)
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveDefault;
	}

	private int ChildrenInGrowthVatCount(Pawn pawn)
	{
		int num = 0;
		if (pawn.relations.ChildrenCount > 0)
		{
			foreach (Pawn child in pawn.relations.Children)
			{
				if (!child.DevelopmentalStage.Adult() && child.ParentHolder != null && child.ParentHolder is Building_GrowthVat)
				{
					num++;
				}
			}
		}
		return num;
	}

	public override float MoodMultiplier(Pawn p)
	{
		return Mathf.Min(def.stackLimit, ChildrenInGrowthVatCount(p));
	}
}
