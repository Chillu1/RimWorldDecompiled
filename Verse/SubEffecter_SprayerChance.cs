using Verse.Sound;

namespace Verse;

public class SubEffecter_SprayerChance : SubEffecter_Sprayer
{
	private int lastSpawnedTicks;

	private int ticksUntilMote;

	private int lifespanMaxTicks;

	public SubEffecter_SprayerChance(SubEffecterDef def, Effecter parent)
		: base(def, parent)
	{
		ticksUntilMote = def.initialDelayTicks;
		lifespanMaxTicks = Find.TickManager.TicksGame + def.lifespanMaxTicks + ticksUntilMote;
	}

	public override void SubEffectTick(TargetInfo A, TargetInfo B)
	{
		float num = base.EffectiveChancePerTick;
		if (base.EffectiveSpawnLocType == MoteSpawnLocType.RandomCellOnTarget && B.HasThing)
		{
			num *= (float)(B.Thing.def.size.x * B.Thing.def.size.z);
		}
		ticksUntilMote--;
		if (Find.TickManager.TicksGame >= lastSpawnedTicks + def.chancePeriodTicks && Find.TickManager.TicksGame < lifespanMaxTicks && ticksUntilMote <= 0 && Rand.Chance(num))
		{
			lastSpawnedTicks = Find.TickManager.TicksGame;
			MakeMote(A, B);
			if (def.soundDef != null)
			{
				def.soundDef.PlayOneShot(A);
			}
		}
	}
}
