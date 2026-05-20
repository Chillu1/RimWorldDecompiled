using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class CompDissolutionEffect : ThingComp
{
	public virtual void DoDissolutionEffectMap(int amount)
	{
	}

	public virtual void DoDissolutionEffectWorld(int amount, PlanetTile tile)
	{
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckBiotech("Dissolution effect"))
		{
			parent.Destroy();
		}
		else
		{
			base.PostSpawnSetup(respawningAfterLoad);
		}
	}
}
