namespace Verse;

public class PawnRenderNodeWorker_Apparel_Head : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override bool CanDrawNow(PawnRenderNode n, PawnDrawParms parms)
	{
		if (!base.CanDrawNow(n, parms))
		{
			return false;
		}
		if (!parms.flags.FlagSet(PawnRenderFlags.Clothes) || !parms.flags.FlagSet(PawnRenderFlags.Headgear))
		{
			return false;
		}
		if (!HeadgearVisible(parms))
		{
			return false;
		}
		if (parms.Portrait && Prefs.HatsOnlyOnMap)
		{
			return parms.flags.FlagSet(PawnRenderFlags.StylingStation);
		}
		return true;
	}

	public static bool HeadgearVisible(PawnDrawParms parms)
	{
		if (!parms.flags.FlagSet(PawnRenderFlags.Clothes) || !parms.flags.FlagSet(PawnRenderFlags.Headgear))
		{
			return false;
		}
		if (!parms.Portrait && parms.bed != null && !parms.bed.def.building.bed_showSleeperBody)
		{
			return false;
		}
		if (parms.Portrait && Prefs.HatsOnlyOnMap)
		{
			return parms.flags.FlagSet(PawnRenderFlags.StylingStation);
		}
		return true;
	}
}
