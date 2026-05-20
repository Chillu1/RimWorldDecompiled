using Verse;

namespace RimWorld
{
	public class SitePartWorker_ConditionCauser_SunBlocker : SitePartWorker_ConditionCauser
	{
		public override bool IsAvailable()
		{
			if (ModsConfig.IdeologyActive)
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Pawn freeColonistsAndPrisoner in map.mapPawns.FreeColonistsAndPrisoners)
					{
						if ((freeColonistsAndPrisoner.IsFreeNonSlaveColonist || freeColonistsAndPrisoner.IsPrisonerOfColony) && !freeColonistsAndPrisoner.IsQuestLodger())
						{
							Ideo ideo = freeColonistsAndPrisoner.Ideo;
							if (ideo != null && (ideo.HasMeme(MemeDefOf.Darkness) || ideo.HasMeme(MemeDefOf.Tunneler)))
							{
								return false;
							}
						}
					}
				}
			}
			return base.IsAvailable();
		}
	}
}
