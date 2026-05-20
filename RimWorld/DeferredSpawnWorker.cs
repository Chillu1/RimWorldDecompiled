using Verse;

namespace RimWorld;

public abstract class DeferredSpawnWorker : IExposable
{
	public abstract void OnSpawn(Thing thing);

	public void ExposeData()
	{
	}
}
