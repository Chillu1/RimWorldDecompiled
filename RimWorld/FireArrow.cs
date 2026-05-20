using Verse;

namespace RimWorld;

public class FireArrow : Projectile
{
	private static FloatRange FireSizeRange = new FloatRange(0.5f, 0.8f);

	private const int FuelToSpreadOnImpact = 4;

	private const int MaxCellsToSpread = 30;

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		if (!base.Position.GetTerrain(base.Map).IsWater)
		{
			int remainingFuel = 4;
			base.Map.floodFiller.FloodFill(base.Position, (IntVec3 c) => c.InBounds(base.Map) && !c.Filled(base.Map), delegate(IntVec3 c)
			{
				foreach (Thing thing in c.GetThingList(base.Map))
				{
					if (thing.def == ThingDefOf.Filth_Fuel)
					{
						return false;
					}
				}
				if (FilthMaker.TryMakeFilth(c, base.Map, ThingDefOf.Filth_Fuel))
				{
					remainingFuel--;
				}
				return remainingFuel <= 0;
			}, 30);
			FireUtility.TryStartFireIn(base.Position, base.Map, FireSizeRange.RandomInRange, launcher);
		}
		FleckCreationData dataStatic = FleckMaker.GetDataStatic(DrawPos, base.Map, FleckDefOf.MicroSparksFast);
		dataStatic.velocitySpeed = 0.8f;
		dataStatic.velocityAngle = ExactRotation.eulerAngles.y;
		base.Map.flecks.CreateFleck(dataStatic);
		base.Impact(hitThing, blockedByShield);
	}
}
