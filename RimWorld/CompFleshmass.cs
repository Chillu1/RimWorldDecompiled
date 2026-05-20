using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompFleshmass : ThingComp, ISizeReporter
{
	public Thing source;

	private Sustainer sustainer;

	public CompProperties_Fleshmass Props => (CompProperties_Fleshmass)props;

	public float CurrentSize()
	{
		return 1f;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_References.Look(ref source, "source");
	}

	public override void CompTickRare()
	{
		if (sustainer == null && !parent.Position.Fogged(parent.Map))
		{
			SoundInfo info = SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map), MaintenanceType.PerTickRare);
			sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, SoundDefOf.FleshmassAmbience, info);
		}
		sustainer?.Maintain();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (sustainer != null)
		{
			if (sustainer.externalParams.sizeAggregator == null)
			{
				sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
			}
			sustainer.externalParams.sizeAggregator.RemoveReporter(this);
		}
		sustainer = null;
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		if ((dinfo?.Instigator?.Faction == null || dinfo.Value.Instigator.Faction == Faction.OfPlayer) && source != null && source.Spawned)
		{
			source.TryGetComp<CompGrowsFleshmassTendrils>()?.Notify_FleshmassDestroyedByPlayer(parent);
		}
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		source.TryGetComp<CompGrowsFleshmassTendrils>()?.Notify_FleshmassDestroyed(parent);
	}

	public override string CompInspectStringExtra()
	{
		CompGrowsFleshmassTendrils compGrowsFleshmassTendrils = source?.TryGetComp<CompGrowsFleshmassTendrils>();
		if (parent.def == ThingDefOf.Fleshmass_Active && compGrowsFleshmassTendrils != null && compGrowsFleshmassTendrils.Props.fleshbeastBirthThresholdRange.HasValue)
		{
			return "ActiveFleshmassInspect".Translate();
		}
		return "";
	}
}
