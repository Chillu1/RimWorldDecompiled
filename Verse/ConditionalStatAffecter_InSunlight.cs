using RimWorld;

namespace Verse
{
	public class ConditionalStatAffecter_InSunlight : ConditionalStatAffecter
	{
		public override string Label => "StatsReport_InSunlight".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			if (req.HasThing && req.Thing.Spawned)
			{
				return req.Thing.Position.InSunlight(req.Thing.Map);
			}
			return false;
		}
	}
}
