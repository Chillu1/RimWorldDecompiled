using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Hunt : PawnColumnWorker_Designator
	{
		protected override DesignationDef DesignationType => DesignationDefOf.Hunt;

		protected override string GetTip(Pawn pawn)
		{
			return "DesignatorHuntDesc".Translate();
		}

		protected override bool HasCheckbox(Pawn pawn)
		{
			if (pawn.AnimalOrWildMan() && pawn.RaceProps.IsFlesh && (pawn.Faction == null || !pawn.Faction.def.humanlikeFaction))
			{
				return pawn.SpawnedOrAnyParentSpawned;
			}
			return false;
		}

		protected override void Notify_DesignationAdded(Pawn pawn)
		{
			pawn.MapHeld.designationManager.TryRemoveDesignationOn(pawn, DesignationDefOf.Tame);
		}
	}
}
