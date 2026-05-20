using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MyParentHappy : ThoughtWorker
	{
		private const float HappyMoodThreshold = 0.6f;

		protected virtual int RequiredParentCount => 1;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return ThoughtState.Inactive;
			}
			if (p.Suspended)
			{
				return ThoughtState.Inactive;
			}
			if (!HasRequiredHappyChildren(p))
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveDefault;
		}

		protected virtual bool HasRequiredHappyChildren(Pawn p)
		{
			return HappyParentCount(p) == RequiredParentCount;
		}

		protected int HappyParentCount(Pawn pawn)
		{
			int num = 0;
			Pawn father = pawn.GetFather();
			Pawn mother = pawn.GetMother();
			if (father != null && ThoughtWorker_RelatedChildMoodBase.InSameMapOrCaravan(pawn, father) && Counts(father))
			{
				num++;
			}
			if (mother != null && ThoughtWorker_RelatedChildMoodBase.InSameMapOrCaravan(pawn, mother) && Counts(mother))
			{
				num++;
			}
			return num;
		}

		public static bool Counts(Pawn pawn)
		{
			if (pawn.needs?.mood != null && pawn.needs.mood.CurLevelPercentage > 0.6f && !(pawn.ParentHolder is Building_GrowthVat))
			{
				return !(pawn.ParentHolder is Building_CryptosleepCasket);
			}
			return false;
		}
	}
}
