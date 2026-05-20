using RimWorld;
using Verse.AI;

namespace Verse;

public class PawnRenderNodeWorker_Body : PawnRenderNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (!base.CanDrawNow(node, parms))
		{
			return false;
		}
		if (!parms.Portrait)
		{
			if (parms.flags.FlagSet(PawnRenderFlags.NoBody))
			{
				return false;
			}
			if (parms.posture == PawnPosture.Standing)
			{
				return true;
			}
			Pawn_MindState mindState = parms.pawn.mindState;
			if (mindState != null && mindState.duty?.def?.drawBodyOverride.HasValue == true)
			{
				return parms.pawn.mindState.duty.def.drawBodyOverride.Value;
			}
			if (parms.bed != null && parms.pawn.RaceProps.Humanlike)
			{
				return parms.bed.def.building.bed_showSleeperBody;
			}
		}
		return true;
	}
}
