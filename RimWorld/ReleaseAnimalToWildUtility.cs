using Verse;

namespace RimWorld
{
	public static class ReleaseAnimalToWildUtility
	{
		public static void CheckWarnAboutBondedAnimal(Pawn designated)
		{
			if (designated.RaceProps.IsFlesh)
			{
				Pawn firstDirectRelationPawn = designated.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => !x.Dead);
				if (firstDirectRelationPawn != null)
				{
					Messages.Message("MessageReleaseBondedAnimal".Translate(designated.LabelShort, firstDirectRelationPawn.LabelShort, designated.Named("DESIGNATED"), firstDirectRelationPawn.Named("BONDED")), designated, MessageTypeDefOf.CautionInput, historical: false);
				}
			}
		}

		public static void DoReleaseAnimal(Pawn animal, Pawn releasedBy)
		{
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(animal, null, PawnDiedOrDownedThoughtsKind.ReleasedToWild);
			releasedBy.interactions.TryInteractWith(animal, InteractionDefOf.ReleaseToWild);
			animal.Map.designationManager.RemoveDesignation(animal.Map.designationManager.DesignationOn(animal, DesignationDefOf.ReleaseAnimalToWild));
			animal.SetFaction(null);
			animal.ownership?.UnclaimAll();
			Messages.Message("MessageAnimalReturnedWildReleased".Translate(animal.LabelShort, animal), releasedBy, MessageTypeDefOf.NeutralEvent);
		}
	}
}
