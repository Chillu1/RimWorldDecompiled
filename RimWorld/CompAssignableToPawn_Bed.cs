using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompAssignableToPawn_Bed : CompAssignableToPawn
{
	public override IEnumerable<Pawn> AssigningCandidates
	{
		get
		{
			if (!parent.Spawned)
			{
				return Enumerable.Empty<Pawn>();
			}
			if (!parent.def.building.bed_humanlike)
			{
				return from p in parent.Map.mapPawns.PawnsInFaction(Faction.OfPlayer)
					where p.IsAnimal && !p.RaceProps.Dryad
					orderby p.kindDef.label, p.Label
					select p;
			}
			return parent.Map.mapPawns.FreeColonists.OrderByDescending(delegate(Pawn p)
			{
				if (!CanAssignTo(p).Accepted)
				{
					return 0;
				}
				return (!IdeoligionForbids(p)) ? 1 : 0;
			}).ThenBy((Pawn p) => p.LabelShort);
		}
	}

	protected override string GetAssignmentGizmoDesc()
	{
		return "CommandBedSetOwnerDesc".Translate(parent.def.building.bed_humanlike ? FactionDefOf.PlayerColony.pawnSingular : "Animal".Translate().ToString());
	}

	public override bool AssignedAnything(Pawn pawn)
	{
		return pawn.ownership.OwnedBed != null;
	}

	public override void TryAssignPawn(Pawn pawn)
	{
		Building_Bed building_Bed = (Building_Bed)parent;
		pawn.ownership.ClaimBedIfNonMedical(building_Bed);
		building_Bed.NotifyRoomAssignedPawnsChanged();
		uninstalledAssignedPawns.Remove(pawn);
	}

	public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
	{
		Building_Bed ownedBed = pawn.ownership.OwnedBed;
		pawn.ownership.UnclaimBed();
		ownedBed?.NotifyRoomAssignedPawnsChanged();
		if (uninstall && !uninstalledAssignedPawns.Contains(pawn))
		{
			uninstalledAssignedPawns.Add(pawn);
		}
	}

	protected override bool ShouldShowAssignmentGizmo()
	{
		Building_Bed building_Bed = (Building_Bed)parent;
		if (building_Bed.Faction == Faction.OfPlayer && !building_Bed.ForPrisoners)
		{
			return !building_Bed.Medical;
		}
		return false;
	}

	public override AcceptanceReport CanAssignTo(Pawn pawn)
	{
		Building_Bed building_Bed = (Building_Bed)parent;
		if (pawn.BodySize > building_Bed.def.building.bed_maxBodySize)
		{
			return "TooLargeForBed".Translate();
		}
		if (!pawn.DevelopmentalStage.Baby())
		{
			if (building_Bed.ForSlaves && !pawn.IsSlave)
			{
				return "CannotAssignBedToColonist".Translate();
			}
			if (building_Bed.ForColonists && pawn.IsSlave)
			{
				return "CannotAssignBedToSlave".Translate();
			}
		}
		return AcceptanceReport.WasAccepted;
	}

	protected override bool CanSetUninstallAssignedPawn(Pawn pawn)
	{
		if (pawn != null && !AssignedAnything(pawn) && (bool)CanAssignTo(pawn))
		{
			if (!pawn.IsPrisonerOfColony)
			{
				return pawn.IsColonist;
			}
			return true;
		}
		return false;
	}

	public override bool IdeoligionForbids(Pawn pawn)
	{
		if (!ModsConfig.IdeologyActive || base.Props.maxAssignedPawnsCount == 1)
		{
			return base.IdeoligionForbids(pawn);
		}
		foreach (Pawn assignedPawn in base.AssignedPawns)
		{
			if (!BedUtility.WillingToShareBed(pawn, assignedPawn))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void PostPostExposeData()
	{
		if (Scribe.mode == LoadSaveMode.PostLoadInit && assignedPawns.RemoveAll((Pawn x) => x.ownership.OwnedBed != parent) > 0)
		{
			Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned bed. Removing.");
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		PostPostExposeData();
	}
}
