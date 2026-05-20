using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompRechargeable : ThingComp
{
	private int ticksUntilCharged;

	private Effecter progressBar;

	private CompPowerTrader compPowerCached;

	private CompProperties_Rechargeable Props => (CompProperties_Rechargeable)props;

	public bool Charged => ticksUntilCharged == 0;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		ticksUntilCharged = Props.ticksToRecharge;
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.WillReplace)
	{
		if (progressBar != null)
		{
			progressBar.Cleanup();
			progressBar = null;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (compPowerCached == null)
		{
			compPowerCached = parent.TryGetComp<CompPowerTrader>();
		}
		if (ticksUntilCharged > 0 && compPowerCached.PowerOn)
		{
			if (ticksUntilCharged == 1 && Props.chargedSoundDef != null)
			{
				Props.chargedSoundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			}
			ticksUntilCharged--;
		}
		if (!Charged && parent.Spawned)
		{
			if (progressBar == null)
			{
				progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
			}
			progressBar.EffectTick(parent, TargetInfo.Invalid);
			MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
			if (mote != null)
			{
				mote.progress = 1f - Mathf.Clamp01((float)ticksUntilCharged / (float)Props.ticksToRecharge);
				mote.offsetZ = -0.5f;
			}
		}
		else if (progressBar != null)
		{
			progressBar.Cleanup();
			progressBar = null;
		}
	}

	public void Discharge()
	{
		ticksUntilCharged = Props.ticksToRecharge;
		if (Props.dischargeSoundDef != null)
		{
			Props.dischargeSoundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		if (!Charged)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Recharge";
			command_Action.action = delegate
			{
				if (Props.chargedSoundDef != null)
				{
					Props.chargedSoundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
				}
				ticksUntilCharged = 0;
			};
			yield return command_Action;
		}
		else
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Discharge";
			command_Action2.action = Discharge;
			yield return command_Action2;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (Charged)
		{
			return "RechargeableReady".Translate() + ".";
		}
		return "RechargeableCharging".Translate() + ": " + "DurationLeft".Translate(ticksUntilCharged.ToStringTicksToPeriod(allowSeconds: true, shortForm: true)) + ".";
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ticksUntilCharged, "ticksUntilCharged", 0);
	}
}
