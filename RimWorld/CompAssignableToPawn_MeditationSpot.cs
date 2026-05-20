using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompAssignableToPawn_MeditationSpot : CompAssignableToPawn
{
	public override IEnumerable<Pawn> AssigningCandidates
	{
		get
		{
			if (!parent.Spawned)
			{
				return Enumerable.Empty<Pawn>();
			}
			return parent.Map.mapPawns.FreeColonists.OrderByDescending((Pawn p) => CanAssignTo(p).Accepted);
		}
	}

	protected override string GetAssignmentGizmoDesc()
	{
		return "CommandMeditationSpotSetOwnerDesc".Translate();
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
		return pawn.ownership.AssignedMeditationSpot != null;
	}

	public override void TryAssignPawn(Pawn pawn)
	{
		pawn.ownership.ClaimMeditationSpot((Building)parent);
	}

	public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
	{
		pawn.ownership.UnclaimMeditationSpot();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit && assignedPawns.RemoveAll((Pawn x) => x.ownership.AssignedMeditationSpot != parent) > 0)
		{
			Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned meditation spot. Removing.");
		}
	}
}
