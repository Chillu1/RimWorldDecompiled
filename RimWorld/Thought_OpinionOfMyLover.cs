using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_OpinionOfMyLover : Thought_Situational
	{
		public override string LabelCap
		{
			get
			{
				DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, allowDead: false);
				string text = base.CurStage.label.Formatted(directPawnRelation.def.GetGenderSpecificLabel(directPawnRelation.otherPawn), directPawnRelation.otherPawn.LabelShort, directPawnRelation.otherPawn).CapitalizeFirst();
				if (def.Worker != null)
				{
					text = def.Worker.PostProcessLabel(pawn, text);
				}
				return text;
			}
		}

		protected override float BaseMoodOffset
		{
			get
			{
				float num = 0.1f * (float)pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, allowDead: false).otherPawn);
				if (num < 0f)
				{
					return Mathf.Min(num, -1f);
				}
				return Mathf.Max(num, 1f);
			}
		}
	}
}
