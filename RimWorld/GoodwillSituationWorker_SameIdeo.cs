using Verse;

namespace RimWorld;

public class GoodwillSituationWorker_SameIdeo : GoodwillSituationWorker
{
	public override string GetPostProcessedLabel(Faction other)
	{
		return def.label.Formatted(Faction.OfPlayer.ideos.PrimaryIdeo);
	}

	public override int GetNaturalGoodwillOffset(Faction other)
	{
		if (Find.IdeoManager.classicMode)
		{
			return 0;
		}
		Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
		if (primaryIdeo == null || primaryIdeo != other.ideos.PrimaryIdeo)
		{
			return 0;
		}
		return def.naturalGoodwillOffset;
	}
}
