using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_WantToSleepWithSpouseOrLover : Thought_Situational
	{
		public override string LabelCap
		{
			get
			{
				DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, allowDead: false);
				string text = base.CurStage.label.Formatted(directPawnRelation.otherPawn.LabelShort, directPawnRelation.otherPawn).CapitalizeFirst();
				if (def.Worker != null)
				{
					text = def.Worker.PostProcessLabel(pawn, text);
				}
				return text;
			}
		}

		protected override float BaseMoodOffset => Mathf.Min(-0.05f * (float)pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, allowDead: false).otherPawn), -1f);
	}
}
