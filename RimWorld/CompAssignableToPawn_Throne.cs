using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompAssignableToPawn_Throne : CompAssignableToPawn
{
	public override IEnumerable<Pawn> AssigningCandidates
	{
		get
		{
			if (!parent.Spawned)
			{
				return Enumerable.Empty<Pawn>();
			}
			Faction faction;
			return from p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
				where p.royalty != null && (p.royalty.AllTitlesForReading.Any() || p.royalty.CanUpdateTitleOfAnyFaction(out faction))
				orderby CanAssignTo(p).Accepted descending
				select p;
		}
	}

	protected override string GetAssignmentGizmoDesc()
	{
		return "CommandThroneSetOwnerDesc".Translate();
	}

	protected override bool CanSetUninstallAssignedPawn(Pawn pawn)
	{
		if (pawn != null && !AssignedAnything(pawn) && (bool)CanAssignTo(pawn))
		{
			return pawn.IsColonist;
		}
		return false;
	}

	public override string CompInspectStringExtra()
	{
		if (base.AssignedPawnsForReading.Count == 0)
		{
			return "Owner".Translate() + ": " + "Nobody".Translate();
		}
		if (base.AssignedPawnsForReading.Count == 1)
		{
			return "Owner".Translate() + ": " + base.AssignedPawnsForReading[0].Label;
		}
		return "";
	}

	public override bool AssignedAnything(Pawn pawn)
	{
		return pawn.ownership.AssignedThrone != null;
	}

	public override void TryAssignPawn(Pawn pawn)
	{
		pawn.ownership.ClaimThrone((Building_Throne)parent);
	}

	public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
	{
		pawn.ownership.UnclaimThrone();
		if (uninstall && pawn != null && !uninstalledAssignedPawns.Contains(pawn))
		{
			uninstalledAssignedPawns.Add(pawn);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit && assignedPawns.RemoveAll((Pawn x) => x.ownership.AssignedThrone != parent) > 0)
		{
			Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned throne. Removing.");
		}
	}
}
