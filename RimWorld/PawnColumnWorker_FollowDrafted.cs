using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_FollowDrafted : PawnColumnWorker_Checkbox
	{
		protected override bool HasCheckbox(Pawn pawn)
		{
			if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
			{
				return pawn.training.HasLearned(TrainableDefOf.Obedience);
			}
			return false;
		}

		protected override bool GetValue(Pawn pawn)
		{
			return pawn.playerSettings.followDrafted;
		}

		protected override void SetValue(Pawn pawn, bool value)
		{
			pawn.playerSettings.followDrafted = value;
		}
	}
}
