using Verse;

namespace RimWorld;

public class CompDestroyAfterEffect : ThingComp
{
	public int destructionTick = int.MinValue;

	private Effecter effecter;

	public CompProperties_DestroyAfterEffect Props => (CompProperties_DestroyAfterEffect)props;

	public override void CompTick()
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (effecter == null)
		{
			effecter = Props.effecterDef.SpawnMaintained(parent.PositionHeld, parent.Map);
			if (destructionTick < 0)
			{
				destructionTick = ticksGame + Props.effecterDef.maintainTicks;
			}
		}
		if (ticksGame >= destructionTick)
		{
			parent.Destroy();
		}
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref destructionTick, "destructionTick", 0);
	}
}
