using Verse;

namespace RimWorld
{
	public class CompAssignableToPawn_DeathrestCasket : CompAssignableToPawn_Bed
	{
		public override string CompInspectStringExtra()
		{
			return null;
		}

		public override bool AssignedAnything(Pawn pawn)
		{
			return pawn.ownership.AssignedDeathrestCasket != null;
		}

		public override void TryAssignPawn(Pawn pawn)
		{
			Building_Bed building_Bed = (Building_Bed)parent;
			pawn.ownership.ClaimDeathrestCasket(building_Bed);
			building_Bed.NotifyRoomAssignedPawnsChanged();
		}

		public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
		{
			Building_Bed ownedBed = pawn.ownership.OwnedBed;
			pawn.ownership.UnclaimDeathrestCasket();
			ownedBed?.NotifyRoomAssignedPawnsChanged();
		}

		public override AcceptanceReport CanAssignTo(Pawn pawn)
		{
			Building_Bed building_Bed = (Building_Bed)parent;
			if (pawn.BodySize > building_Bed.def.building.bed_maxBodySize)
			{
				return "TooLargeForBed".Translate();
			}
			if (building_Bed.ForSlaves && !pawn.IsSlave)
			{
				return "CannotAssignBedToColonist".Translate();
			}
			if (building_Bed.ForColonists && pawn.IsSlave)
			{
				return "CannotAssignBedToSlave".Translate();
			}
			CompDeathrestBindable compDeathrestBindable = parent.TryGetComp<CompDeathrestBindable>();
			if (compDeathrestBindable != null && compDeathrestBindable.BoundPawn != null && compDeathrestBindable.BoundPawn != pawn)
			{
				return "CannotAssignAlreadyBound".Translate(compDeathrestBindable.BoundPawn);
			}
			Gene_Deathrest gene_Deathrest = pawn.genes?.GetFirstGeneOfType<Gene_Deathrest>();
			if (gene_Deathrest == null)
			{
				return "CannotAssignBedCannotDeathrest".Translate();
			}
			if (compDeathrestBindable != null && gene_Deathrest.BindingWillExceedStackLimit(compDeathrestBindable))
			{
				return "CannotAssignBedCannotBindToMoreBuildings".Translate(NamedArgumentUtility.Named(parent.def, "BUILDING"));
			}
			return AcceptanceReport.WasAccepted;
		}

		protected override void PostPostExposeData()
		{
			if (Scribe.mode == LoadSaveMode.PostLoadInit && ModsConfig.BiotechActive && assignedPawns.RemoveAll((Pawn x) => x.ownership.AssignedDeathrestCasket != parent) > 0)
			{
				Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned bed. Removing.");
			}
		}
	}
}
