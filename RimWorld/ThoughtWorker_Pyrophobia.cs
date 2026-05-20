using Verse;

namespace RimWorld;

public class ThoughtWorker_Pyrophobia : ThoughtWorker
{
	public const float FireRadius = 19.9f;

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (p.genes == null || !p.genes.HasActiveGene(GeneDefOf.FireTerror) || !NearFire(p))
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveAtStage(0);
	}

	public static bool NearFire(Pawn pawn)
	{
		Map mapHeld = pawn.MapHeld;
		if (mapHeld == null)
		{
			return false;
		}
		IntVec3 positionHeld = pawn.PositionHeld;
		int num = GenRadial.NumCellsInRadius(19.9f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
			if (intVec.InBounds(mapHeld) && !intVec.Fogged(mapHeld) && GenSight.LineOfSight(positionHeld, intVec, mapHeld, skipFirstCell: true) && intVec.ContainsStaticFire(mapHeld))
			{
				return true;
			}
		}
		return false;
	}
}
