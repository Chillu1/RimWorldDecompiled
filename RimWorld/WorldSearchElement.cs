using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class WorldSearchElement
{
	public PlanetTile tile = PlanetTile.Invalid;

	public WorldObject worldObject;

	public Landmark landmark;

	public List<TileMutatorDef> mutators;

	public string DisplayLabel
	{
		get
		{
			if (worldObject != null)
			{
				return worldObject.LabelCap;
			}
			if (ModsConfig.OdysseyActive)
			{
				if (landmark != null)
				{
					return landmark.name.CapitalizeFirst();
				}
				if (!mutators.NullOrEmpty())
				{
					return mutators[0].label.CapitalizeFirst();
				}
			}
			return null;
		}
	}
}
