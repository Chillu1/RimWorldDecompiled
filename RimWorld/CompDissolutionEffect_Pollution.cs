using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompDissolutionEffect_Pollution : CompDissolutionEffect
{
	private struct WorldPollutionEvent
	{
		public PlanetTile tile;

		public float amount;
	}

	private static List<WorldPollutionEvent> pendingWorldEvents = new List<WorldPollutionEvent>();

	public CompProperties_DissolutionEffectPollution Props => (CompProperties_DissolutionEffectPollution)props;

	public override void DoDissolutionEffectMap(int amount)
	{
		PollutionUtility.GrowPollutionAt(parent.PositionHeld, parent.MapHeld, amount * Props.cellsToPollutePerDissolution);
		FleckMaker.Static(parent.PositionHeld, parent.MapHeld, FleckDefOf.Fleck_WastePackDissolutionSource);
		SoundDefOf.WastepackDissolution.PlayOneShot(parent);
	}

	public override void DoDissolutionEffectWorld(int amount, PlanetTile tileId)
	{
		if (!tileId.Valid)
		{
			return;
		}
		Tile tile = Find.World.grid[tileId];
		if (tile != null)
		{
			float num = Props.tilePollutionPerDissolution * (float)amount;
			if (tile.WaterCovered)
			{
				num *= Props.waterTilePollutionFactor;
			}
			WorldPollutionEvent item = new WorldPollutionEvent
			{
				tile = tileId,
				amount = num
			};
			pendingWorldEvents.Add(item);
		}
	}

	public static void WorldUpdate()
	{
		if (pendingWorldEvents.Count <= 0)
		{
			return;
		}
		foreach (IGrouping<PlanetTile, WorldPollutionEvent> item in from e in pendingWorldEvents
			group e by e.tile)
		{
			WorldPollutionUtility.PolluteWorldAtTile(item.Key, item.Sum((WorldPollutionEvent e) => e.amount));
		}
		pendingWorldEvents.Clear();
	}
}
