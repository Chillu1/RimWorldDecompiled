using System.Collections.Generic;
using System.Linq;

namespace Verse;

public class RitualStagePositions : IExposable
{
	public Dictionary<PawnRitualReference, PawnStagePosition> referencePositions = new Dictionary<PawnRitualReference, PawnStagePosition>();

	private Dictionary<Pawn, PawnStagePosition> positionsTmp = new Dictionary<Pawn, PawnStagePosition>();

	private List<Pawn> pawnListTmp;

	private List<PawnRitualReference> pawnRefListTmp;

	private List<PawnStagePosition> positionListTmp;

	public void ExposeData()
	{
		Scribe_Collections.Look(ref referencePositions, "referencePosition", LookMode.Deep, LookMode.Deep, ref pawnRefListTmp, ref positionListTmp);
		if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
		{
			Scribe_Collections.Look(ref positionsTmp, "positions", LookMode.Reference, LookMode.Deep, ref pawnListTmp, ref positionListTmp);
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		Scribe_Collections.Look(ref positionsTmp, "positions", LookMode.Reference, LookMode.Deep, ref pawnListTmp, ref positionListTmp);
		if (positionsTmp == null || positionsTmp.Count <= 0)
		{
			return;
		}
		if (referencePositions == null)
		{
			referencePositions = new Dictionary<PawnRitualReference, PawnStagePosition>();
		}
		foreach (KeyValuePair<Pawn, PawnStagePosition> pawnPos in positionsTmp)
		{
			if (referencePositions.All((KeyValuePair<PawnRitualReference, PawnStagePosition> p) => p.Key.pawn != pawnPos.Key))
			{
				referencePositions.Add(new PawnRitualReference(pawnPos.Key), pawnPos.Value);
			}
		}
	}
}
