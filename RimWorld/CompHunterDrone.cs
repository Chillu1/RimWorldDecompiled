using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompHunterDrone : ThingComp
{
	private bool wickStarted;

	private int wickTicks;

	[Unsaved(false)]
	private Sustainer wickSoundSustainer;

	[Unsaved(false)]
	private OverlayHandle? overlayBurningWick;

	private CompProperties_HunterDrone Props => (CompProperties_HunterDrone)props;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref wickStarted, "wickStarted", defaultValue: false);
		Scribe_Values.Look(ref wickTicks, "wickTicks", 0);
	}

	public override void CompTickInterval(int delta)
	{
		if (!wickStarted && parent.IsHashIntervalTick(30, delta) && parent is Pawn { Spawned: not false, Downed: false } pawn && PawnUtility.EnemiesAreNearby(pawn, 9, passDoors: true, 1.5f))
		{
			StartWick();
		}
	}

	public override void CompTick()
	{
		if (wickStarted)
		{
			wickSoundSustainer.Maintain();
			wickTicks--;
			if (wickTicks <= 0)
			{
				Detonate();
			}
		}
	}

	private void StartWick()
	{
		if (!wickStarted)
		{
			wickStarted = true;
			overlayBurningWick = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.BurningWick);
			wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
			wickTicks = 120;
		}
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		if (dinfo.HasValue)
		{
			Detonate(prevMap);
		}
	}

	private void Detonate(Map map = null)
	{
		IntVec3 position = parent.Position;
		if (map == null)
		{
			map = parent.Map;
		}
		if (!parent.Destroyed)
		{
			parent.Destroy();
		}
		GenExplosion.DoExplosion(position, map, Props.explosionRadius, Props.explosionDamageType, parent, Props.explosionDamageAmount);
		if (base.ParentHolder is Corpse corpse)
		{
			corpse.Destroy();
		}
	}
}
