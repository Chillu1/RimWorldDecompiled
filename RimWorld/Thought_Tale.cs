using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_Tale : Thought_SituationalSocial
	{
		protected override ThoughtState CurrentStateInternal()
		{
			if (ThoughtUtility.ThoughtNullified(pawn, def))
			{
				return false;
			}
			return def.Worker.CurrentSocialState(pawn, otherPawn);
		}

		public override float OpinionOffset()
		{
			if (ThoughtUtility.ThoughtNullified(pawn, def))
			{
				return 0f;
			}
			Tale latestTale = Find.TaleManager.GetLatestTale(def.taleDef, otherPawn);
			if (latestTale != null)
			{
				float num = 1f;
				if (latestTale.def.type == TaleType.Expirable)
				{
					float value = (float)latestTale.AgeTicks / (latestTale.def.expireDays * 60000f);
					num = Mathf.InverseLerp(1f, def.lerpOpinionToZeroAfterDurationPct, value);
				}
				return base.CurStage.baseOpinionOffset * num;
			}
			return 0f;
		}
	}
}
