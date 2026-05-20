using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompLoudspeaker : CompAutoPowered, IThingGlower
{
	public const int Range = 12;

	private Mote lightsMote;

	private bool inRitual;

	private bool initialized;

	private CompProperties_Loudspeaker Props => (CompProperties_Loudspeaker)props;

	public bool Active => inRitual;

	public override bool WantsToBeOn => inRitual;

	public bool ShouldBeLitNow()
	{
		return Active;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!ModLister.CheckIdeology("Speaker"))
		{
			return;
		}
		if (compPowerCached == null)
		{
			compPowerCached = parent.GetComp<CompPowerTrader>();
		}
		if (parent.IsHashIntervalTick(30) || !initialized)
		{
			initialized = true;
			UpdateInRitual();
			UpdateOverlays();
		}
		if (compPowerCached.PowerOn && inRitual)
		{
			if (parent.Rotation == Rot4.North && (lightsMote == null || lightsMote.Destroyed))
			{
				lightsMote = MoteMaker.MakeStaticMote(parent.TrueCenter(), parent.Map, ThingDefOf.Mote_LoudspeakerLights);
			}
			lightsMote?.Maintain();
		}
		else
		{
			lightsMote?.Destroy();
			lightsMote = null;
		}
		if (compPowerCached.PowerOn && !inRitual)
		{
			parent.BroadcastCompSignal("AutoPoweredWantsOff");
		}
	}

	public override void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag == "RitualStarted")
		{
			UpdateInRitual();
		}
	}

	private void UpdateInRitual()
	{
		inRitual = false;
		if (!parent.Spawned)
		{
			return;
		}
		foreach (Lord lord in parent.Map.lordManager.lords)
		{
			if (lord.LordJob is LordJob_Ritual lordJob_Ritual && lordJob_Ritual.selectedTarget.Thing != null && lordJob_Ritual.selectedTarget.Thing.def == ThingDefOf.LightBall && parent.GetRoom() == lordJob_Ritual.selectedTarget.Thing.GetRoom())
			{
				inRitual = true;
				break;
			}
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		PlaceWorker_ShowSpeakerConnections.DrawConnections(parent.def, parent.Position, parent.Map);
	}
}
