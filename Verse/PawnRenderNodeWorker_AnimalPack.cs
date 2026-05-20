namespace Verse;

public class PawnRenderNodeWorker_AnimalPack : PawnRenderNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms) && !parms.Portrait && parms.pawn.RaceProps.Animal && parms.pawn.inventory != null)
		{
			return parms.pawn.inventory.innerContainer.Count > 0;
		}
		return false;
	}
}
