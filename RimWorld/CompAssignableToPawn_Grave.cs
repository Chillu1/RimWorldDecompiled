using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompAssignableToPawn_Grave : CompAssignableToPawn
{
	public override IEnumerable<Pawn> AssigningCandidates
	{
		get
		{
			if (!parent.Spawned)
			{
				return Enumerable.Empty<Pawn>();
			}
			IEnumerable<Pawn> second = from Corpse x in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
				where x.InnerPawn.IsColonist
				select x.InnerPawn;
			return parent.Map.mapPawns.FreeColonistsSpawned.Concat(second);
		}
	}

	public override bool AssignedAnything(Pawn pawn)
	{
		return pawn.ownership.AssignedGrave != null;
	}

	public override void TryAssignPawn(Pawn pawn)
	{
		pawn.ownership.ClaimGrave((Building_Grave)parent);
	}

	public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
	{
		pawn.ownership.UnclaimGrave();
	}

	protected override bool ShouldShowAssignmentGizmo()
	{
		return !((Building_Grave)parent).HasCorpse;
	}

	protected override string GetAssignmentGizmoLabel()
	{
		return "CommandGraveAssignColonistLabel".Translate();
	}

	protected override string GetAssignmentGizmoDesc()
	{
		return "CommandGraveAssignColonistDesc".Translate();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit && assignedPawns.RemoveAll((Pawn x) => x.ownership.AssignedGrave != parent) > 0)
		{
			Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned grave. Removing.");
		}
	}
}
