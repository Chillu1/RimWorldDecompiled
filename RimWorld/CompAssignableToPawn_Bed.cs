using Verse;

namespace RimWorld
{
	public class CompAssignableToPawn_Bed : CompAssignableToPawn
	{
		protected override string GetAssignmentGizmoDesc()
		{
			return "CommandBedSetOwnerDesc".Translate();
		}

		public override bool AssignedAnything(Pawn pawn)
		{
			return pawn.ownership.OwnedBed != null;
		}

		public override void TryAssignPawn(Pawn pawn)
		{
			pawn.ownership.ClaimBedIfNonMedical((Building_Bed)parent);
		}

		public override void TryUnassignPawn(Pawn pawn, bool sort = true)
		{
			pawn.ownership.UnclaimBed();
		}

		protected override bool ShouldShowAssignmentGizmo()
		{
			Building_Bed building_Bed = (Building_Bed)parent;
			if (building_Bed.def.building.bed_humanlike && building_Bed.Faction == Faction.OfPlayer && !building_Bed.ForPrisoners)
			{
				return !building_Bed.Medical;
			}
			return false;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && assignedPawns.RemoveAll((Pawn x) => x.ownership.OwnedBed != parent) > 0)
			{
				Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned bed. Removing.");
			}
		}
	}
}
