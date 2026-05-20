using Verse;

namespace RimWorld;

public class PawnColumnWorker_Slaughter : PawnColumnWorker_Designator
{
	protected override DesignationDef DesignationType => DesignationDefOf.Slaughter;

	protected override string GetTip(Pawn pawn)
	{
		return "DesignatorSlaughterDesc".Translate();
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
		SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(pawn);
	}

	protected override bool ShouldConfirmDesignation(Pawn pawn, out string title)
	{
		if (pawn.HomeFaction == Faction.OfPlayer || pawn.HomeFaction == null)
		{
			return base.ShouldConfirmDesignation(pawn, out title);
		}
		title = "AnimalSlaughterConfirm".Translate(pawn.Named("PAWN"), pawn.HomeFaction.Named("FACTION"));
		return true;
	}
}
