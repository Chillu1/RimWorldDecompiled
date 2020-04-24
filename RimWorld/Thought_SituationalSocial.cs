using Verse;

namespace RimWorld
{
	public class Thought_SituationalSocial : Thought_Situational, ISocialThought
	{
		public Pawn otherPawn;

		public override bool VisibleInNeedsTab
		{
			get
			{
				if (base.VisibleInNeedsTab)
				{
					return MoodOffset() != 0f;
				}
				return false;
			}
		}

		public Pawn OtherPawn()
		{
			return otherPawn;
		}

		public virtual float OpinionOffset()
		{
			if (ThoughtUtility.ThoughtNullified(pawn, def))
			{
				return 0f;
			}
			float num = base.CurStage.baseOpinionOffset;
			if (def.effectMultiplyingStat != null)
			{
				num *= pawn.GetStatValue(def.effectMultiplyingStat) * otherPawn.GetStatValue(def.effectMultiplyingStat);
			}
			return num;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_SituationalSocial thought_SituationalSocial = other as Thought_SituationalSocial;
			if (thought_SituationalSocial == null)
			{
				return false;
			}
			if (base.GroupsWith(other))
			{
				return otherPawn == thought_SituationalSocial.otherPawn;
			}
			return false;
		}

		protected override ThoughtState CurrentStateInternal()
		{
			return def.Worker.CurrentSocialState(pawn, otherPawn);
		}
	}
}
