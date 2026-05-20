using System.Collections.Generic;

namespace Verse;

public class SubEffecter_GroupedChance : SubEffecter
{
	private int lastEffectTicks;

	private int ticksUntilEffect;

	private int lifespanMaxTicks;

	public List<SubEffecter> children = new List<SubEffecter>();

	public SubEffecter_GroupedChance(SubEffecterDef subDef, Effecter parent)
		: base(subDef, parent)
	{
		ticksUntilEffect = def.initialDelayTicks;
		lifespanMaxTicks = Find.TickManager.TicksGame + def.lifespanMaxTicks + ticksUntilEffect;
		if (def.children != null)
		{
			for (int i = 0; i < def.children.Count; i++)
			{
				children.Add(def.children[i].Spawn(parent));
			}
		}
	}

	public override void SubEffectTick(TargetInfo A, TargetInfo B)
	{
		ticksUntilEffect--;
		if (Find.TickManager.TicksGame >= lastEffectTicks + def.chancePeriodTicks && Find.TickManager.TicksGame < lifespanMaxTicks && ticksUntilEffect <= 0 && Rand.Chance(base.EffectiveChancePerTick))
		{
			lastEffectTicks = Find.TickManager.TicksGame;
			SubTrigger(A, B, -1, force: true);
		}
	}

	public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
		if (!def.subTriggerOnSpawn && Find.TickManager.TicksGame == parent.spawnTick)
		{
			return;
		}
		foreach (SubEffecter child in children)
		{
			child.SubTrigger(A, B, overrideSpawnTick, force);
		}
	}
}
