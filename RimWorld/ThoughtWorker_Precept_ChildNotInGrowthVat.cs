using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_ChildNotInGrowthVat : ThoughtWorker_Precept
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

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.IdeologyActive || !ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (ChildrenNotInGrowthVatCount(p) <= 0)
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveDefault;
	}

	private int ChildrenNotInGrowthVatCount(Pawn pawn)
	{
		int num = 0;
		if (pawn.relations.ChildrenCount > 0)
		{
			foreach (Pawn child in pawn.relations.Children)
			{
				if (!child.Dead && child.MapHeld == pawn.MapHeld && !child.DevelopmentalStage.Adult() && !(child.ParentHolder is Building_GrowthVat))
				{
					num++;
				}
			}
		}
		return num;
	}

	public override float MoodMultiplier(Pawn p)
	{
		return Mathf.Min(def.stackLimit, ChildrenNotInGrowthVatCount(p));
	}
}
