using System;
using Verse;

namespace RimWorld;

public class CompLightball : CompAutoPowered
{
	private Mote rotationMote;

	private Mote lightsMote;

	private bool inRitual;

	private int numActiveSpeakers = -1;

	private SoundDef soundToPlay;

	private CompProperties_Lightball Props => (CompProperties_Lightball)props;

	public bool Playing => soundToPlay != null;

	public SoundDef SoundToPlay => soundToPlay;

	public override bool WantsToBeOn => inRitual;

	public override void CompTick()
	{
		base.CompTick();
		if (!ModLister.CheckIdeology("Lightball"))
		{
			return;
		}
		if (parent.Spawned && (parent.IsHashIntervalTick(20) || numActiveSpeakers == -1))
		{
			numActiveSpeakers = 0;
			foreach (Thing item in parent.Map.listerBuldingOfDefInProximity.GetForCell(parent.Position, Props.maxSpeakerDistance, ThingDefOf.Loudspeaker))
			{
				CompPowerTrader compPowerTrader = item.TryGetComp<CompPowerTrader>();
				if (item.GetRoom() == parent.GetRoom() && compPowerTrader.PowerOn)
				{
					numActiveSpeakers++;
				}
			}
			UpdateOverlays();
		}
		if (compPowerCached == null)
		{
			compPowerCached = parent.GetComp<CompPowerTrader>();
		}
		if (parent.IsHashIntervalTick(30))
		{
			inRitual = parent.IsRitualTarget();
		}
		if (compPowerCached.PowerOn && !inRitual)
		{
			parent.BroadcastCompSignal("AutoPoweredWantsOff");
		}
		if (compPowerCached.PowerOn && inRitual)
		{
			if (numActiveSpeakers > 0)
			{
				soundToPlay = ((!Props.soundDefsPerSpeakerCount.NullOrEmpty()) ? Props.soundDefsPerSpeakerCount[Math.Min(numActiveSpeakers, Props.soundDefsPerSpeakerCount.Count) - 1] : null);
			}
			if (rotationMote == null || rotationMote.Destroyed)
			{
				rotationMote = MoteMaker.MakeStaticMote(parent.TrueCenter(), parent.Map, ThingDefOf.Mote_LightBall);
			}
			rotationMote?.Maintain();
			if (lightsMote == null || lightsMote.Destroyed)
			{
				lightsMote = MoteMaker.MakeStaticMote(parent.TrueCenter(), parent.Map, ThingDefOf.Mote_LightBallLights);
				if (lightsMote != null)
				{
					lightsMote.rotationRate = -3f;
				}
			}
			lightsMote?.Maintain();
		}
		else
		{
			rotationMote?.Destroy();
			rotationMote = null;
			lightsMote?.Destroy();
			lightsMote = null;
		}
	}

	public override void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag == "RitualStarted")
		{
			inRitual = parent.IsRitualTarget();
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (respawningAfterLoad)
		{
			inRitual = parent.IsRitualTarget();
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		PlaceWorker_ShowSpeakerConnections.DrawConnections(parent.def, parent.Position, parent.Map);
	}
}
