using Verse;

namespace RimWorld;

public class CompFleshmassHeartChild : ThingComp
{
	private static readonly IntRange DestroyDelayRangeTicks = new IntRange(180, 480);

	public Building_FleshmassHeart heart;

	private int destroyTick = -99999;

	public CompFleshmassHeart HeartComp => heart?.GetComp<CompFleshmassHeart>();

	public override void PostExposeData()
	{
		Scribe_References.Look(ref heart, "heart");
	}

	public override void CompTick()
	{
		if (destroyTick > 0 && Find.TickManager.TicksGame >= destroyTick)
		{
			parent.Destroy();
		}
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		HeartComp?.Notify_ChildDied(parent);
	}

	public void Notify_HeartDied()
	{
		destroyTick = Find.TickManager.TicksGame + DestroyDelayRangeTicks.RandomInRange;
	}
}
