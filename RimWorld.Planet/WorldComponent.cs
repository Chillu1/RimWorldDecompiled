using Verse;

namespace RimWorld.Planet;

public abstract class WorldComponent : IExposable
{
	public World world;

	public WorldComponent(World world)
	{
		this.world = world;
	}

	public virtual void WorldComponentUpdate()
	{
	}

	public virtual void WorldComponentTick()
	{
	}

	public virtual void WorldComponentOnGUI()
	{
	}

	public virtual void ExposeData()
	{
		BackCompatibility.PostExposeData(this);
	}

	public virtual void FinalizeInit(bool fromLoad)
	{
	}
}
