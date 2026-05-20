using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class MixedBiomeMapComponent : MapComponent
{
	public bool[] biomeGrid;

	public BiomeDef secondaryBiome;

	public bool IsMixedBiome => biomeGrid != null;

	public MixedBiomeMapComponent(Map map)
		: base(map)
	{
	}

	public BiomeDef GetBiomeAt(IntVec3 cell)
	{
		if (biomeGrid == null || !ModsConfig.OdysseyActive)
		{
			return map.Biome;
		}
		if (!cell.InBounds(map))
		{
			return null;
		}
		int num = map.cellIndices.CellToIndex(cell);
		if (!biomeGrid[num])
		{
			return map.Biome;
		}
		return secondaryBiome;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		List<bool> list = biomeGrid?.ToList();
		Scribe_Collections.Look(ref list, "biomeGrid", LookMode.Undefined);
		biomeGrid = list?.ToArray();
		Scribe_Defs.Look(ref secondaryBiome, "secondaryBiome");
	}
}
