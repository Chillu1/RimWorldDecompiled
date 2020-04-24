namespace RimWorld.Planet
{
	public class SitePartWorker_Ambush : SitePartWorker
	{
		private const float ThreatPointsFactor = 0.8f;

		public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
		{
			SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
			sitePartParams.threatPoints *= 0.8f;
			return sitePartParams;
		}
	}
}
