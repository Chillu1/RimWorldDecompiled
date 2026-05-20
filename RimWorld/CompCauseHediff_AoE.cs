using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompCauseHediff_AoE : ThingComp
{
	public float range;

	private Sustainer activeSustainer;

	private bool lastIntervalActive;

	public CompProperties_CauseHediff_AoE Props => (CompProperties_CauseHediff_AoE)props;

	private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();

	private bool IsPawnAffected(Pawn target)
	{
		if (PowerTrader != null && !PowerTrader.PowerOn)
		{
			return false;
		}
		if (target.Dead || target.health == null)
		{
			return false;
		}
		if (target == parent && !Props.canTargetSelf)
		{
			return false;
		}
		if (Props.ignoreMechs && target.RaceProps.IsMechanoid)
		{
			return false;
		}
		if (!Props.onlyTargetMechs || target.RaceProps.IsMechanoid)
		{
			return target.PositionHeld.DistanceTo(parent.PositionHeld) <= range;
		}
		return false;
	}

	public override void PostPostMake()
	{
		base.PostPostMake();
		range = Props.range;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref range, "range", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && range <= 0f)
		{
			range = Props.range;
		}
	}

	public override void CompTick()
	{
		MaintainSustainer();
		if (!parent.IsHashIntervalTick(Props.checkInterval))
		{
			return;
		}
		CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
		if (compPowerTrader != null && !compPowerTrader.PowerOn)
		{
			return;
		}
		lastIntervalActive = false;
		if (!parent.SpawnedOrAnyParentSpawned)
		{
			return;
		}
		foreach (Pawn item in parent.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (IsPawnAffected(item))
			{
				GiveOrUpdateHediff(item);
			}
			if (item.carryTracker.CarriedThing is Pawn target && IsPawnAffected(target))
			{
				GiveOrUpdateHediff(target);
			}
		}
	}

	private void GiveOrUpdateHediff(Pawn target)
	{
		Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
		if (hediff == null)
		{
			hediff = target.health.AddHediff(Props.hediff, target.health.hediffSet.GetBrain());
			hediff.Severity = 1f;
			HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
			if (hediffComp_Link != null)
			{
				hediffComp_Link.drawConnection = false;
				hediffComp_Link.other = parent;
			}
		}
		HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
		if (hediffComp_Disappears == null)
		{
			Log.ErrorOnce("CompCauseHediff_AoE has a hediff in props which does not have a HediffComp_Disappears", 78945945);
		}
		else
		{
			hediffComp_Disappears.ticksToDisappear = Props.checkInterval + 5;
		}
		lastIntervalActive = true;
	}

	private void MaintainSustainer()
	{
		if (lastIntervalActive && Props.activeSound != null)
		{
			if (activeSustainer == null || activeSustainer.Ended)
			{
				activeSustainer = Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(parent)));
			}
			activeSustainer.Maintain();
		}
		else if (activeSustainer != null)
		{
			activeSustainer.End();
			activeSustainer = null;
		}
	}

	public override void PostDraw()
	{
		if (!Props.drawLines)
		{
			return;
		}
		int num = Mathf.Max(parent.Map.Size.x, parent.Map.Size.y);
		if (!Find.Selector.SelectedObjectsListForReading.Contains(parent) || !(range < (float)num))
		{
			return;
		}
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
		{
			if (IsPawnAffected(item))
			{
				GenDraw.DrawLineBetween(item.DrawPos, parent.DrawPos);
			}
		}
	}
}
