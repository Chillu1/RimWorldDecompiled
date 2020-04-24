using Verse;

namespace RimWorld
{
	public class CompPowerPlantSteam : CompPowerPlant
	{
		private IntermittentSteamSprayer steamSprayer;

		private Building_SteamGeyser geyser;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			steamSprayer = new IntermittentSteamSprayer(parent);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (geyser == null)
			{
				geyser = (Building_SteamGeyser)parent.Map.thingGrid.ThingAt(parent.Position, ThingDefOf.SteamGeyser);
			}
			if (geyser != null)
			{
				geyser.harvester = (Building)parent;
				steamSprayer.SteamSprayerTick();
			}
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (geyser != null)
			{
				geyser.harvester = null;
			}
		}
	}
}
