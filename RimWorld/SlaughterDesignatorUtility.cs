using Verse;

namespace RimWorld;

public static class SlaughterDesignatorUtility
{
	public static void CheckWarnAboutBondedAnimal(Pawn designated)
	{
		if (designated.RaceProps.IsFlesh)
		{
			Pawn firstDirectRelationPawn = designated.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => !x.Dead);
			if (firstDirectRelationPawn != null)
			{
				Messages.Message("MessageSlaughteringBondedAnimal".Translate(designated.LabelShort, firstDirectRelationPawn.LabelShort, designated.Named("DESIGNATED"), firstDirectRelationPawn.Named("BONDED")), designated, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
	}

	public static void CheckWarnAboutVeneratedAnimal(Pawn pawn)
	{
		if (!ModsConfig.IdeologyActive || !pawn.SpawnedOrAnyParentSpawned || pawn.MapHeld.mapPawns.FreeColonistsSpawned.Count == 0)
		{
			return;
		}
		bool flag = true;
		bool flag2 = false;
		foreach (Pawn item in pawn.MapHeld.mapPawns.FreeColonistsSpawned)
		{
			if (!item.WorkTypeIsDisabled(WorkTypeDefOf.Hunting))
			{
				flag2 = true;
				if (item.Ideo == null || !item.Ideo.IsVeneratedAnimal(pawn))
				{
					flag = false;
					break;
				}
			}
		}
		if (flag2 && flag)
		{
			Messages.Message("MessageAnimalIsVeneratedForAllColonists".Translate(pawn.GetKindLabelPlural().CapitalizeFirst().Named("PAWNKINDLABELPLURAL"), Faction.OfPlayer.def.pawnsPlural.Named("PAWNS")), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
	}
}
