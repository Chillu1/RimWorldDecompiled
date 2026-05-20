using Verse;

namespace RimWorld;

public class PawnColumnWorker_ReleaseAnimalToWild : PawnColumnWorker_Designator
{
	protected override DesignationDef DesignationType => DesignationDefOf.ReleaseAnimalToWild;

	protected override string GetTip(Pawn pawn)
	{
		return "DesignatorReleaseAnimalToWildDesc".Translate();
	}

	protected override bool HasCheckbox(Pawn pawn)
	{
		if (pawn.RaceProps.Animal && pawn.RaceProps.IsFlesh && pawn.Faction == Faction.OfPlayer)
		{
			return pawn.SpawnedOrAnyParentSpawned;
		}
		return false;
	}

	protected override void Notify_DesignationAdded(Pawn pawn)
	{
		if (pawn.Faction != Faction.OfPlayer)
		{
			TaggedString taggedString = "AnimalReleaseConfirm".Translate(pawn.Named("PAWN"), pawn.Faction.Named("FACTION"));
			Find.WindowStack.Add(new Dialog_Confirm(taggedString, delegate
			{
				ReleaseAnimalToWildUtility.CheckWarnAboutBondedAnimal(pawn);
			}));
		}
		else
		{
			ReleaseAnimalToWildUtility.CheckWarnAboutBondedAnimal(pawn);
		}
	}

	protected override bool ShouldConfirmDesignation(Pawn pawn, out string title)
	{
		if (pawn.HomeFaction == Faction.OfPlayer || pawn.HomeFaction == null)
		{
			return base.ShouldConfirmDesignation(pawn, out title);
		}
		title = "AnimalReleaseConfirm".Translate(pawn.Named("PAWN"), pawn.HomeFaction.Named("FACTION"));
		return true;
	}
}
