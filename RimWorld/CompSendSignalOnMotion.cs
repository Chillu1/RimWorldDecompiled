using System;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompSendSignalOnMotion : ThingComp, IThingGlower
{
	public string signalTag;

	private bool sent;

	private int ticksUntilEnabled;

	private const float MaxDistActivationByOther = 40f;

	public CompProperties_SendSignalOnMotion Props => (CompProperties_SendSignalOnMotion)props;

	public bool Sent => sent;

	public bool Enabled => ticksUntilEnabled <= 0;

	public bool ShouldBeLitNow()
	{
		return !Sent;
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		signalTag = Props.signalTag;
		ticksUntilEnabled = Props.enableAfterTicks;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!sent && parent.Spawned)
		{
			if (Enabled && Find.TickManager.TicksGame % 250 == 0)
			{
				CompTickRare();
			}
			if (ticksUntilEnabled > 0)
			{
				ticksUntilEnabled--;
			}
		}
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		Predicate<Thing> predicate = null;
		if (Props.onlyHumanlike)
		{
			predicate = (Thing t) => t is Pawn pawn && pawn.RaceProps.Humanlike;
		}
		Thing thing = null;
		if (Props.triggerOnPawnInRoom)
		{
			foreach (Thing containedAndAdjacentThing in parent.GetRoom().ContainedAndAdjacentThings)
			{
				if (predicate(containedAndAdjacentThing))
				{
					thing = containedAndAdjacentThing;
					break;
				}
			}
		}
		if (thing == null && Props.radius > 0f)
		{
			thing = GenClosest.ClosestThingReachable(parent.Position, parent.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.radius, predicate);
		}
		if (thing != null)
		{
			Trigger(thing);
		}
	}

	protected void Trigger(Thing initiator)
	{
		Effecter effecter = new Effecter(EffecterDefOf.ActivatorProximityTriggered);
		effecter.Trigger(parent, TargetInfo.Invalid);
		effecter.Cleanup();
		Messages.Message("MessageActivatorProximityTriggered".Translate(initiator), parent, MessageTypeDefOf.ThreatBig);
		Find.SignalManager.SendSignal(new Signal(signalTag, parent.Named("SUBJECT")));
		SoundDefOf.MechanoidsWakeUp.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
		sent = true;
	}

	public void Expire()
	{
		sent = true;
	}

	public override void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag == "CompCanBeDormant.WakeUp" && signal.args.TryGetArg("SUBJECT", out Thing arg) && arg != parent && arg != null && arg.Map == parent.Map && parent.Position.DistanceTo(arg.Position) <= 40f)
		{
			sent = true;
		}
		if (!sent && signal.tag == CompAbilityEffect_Teleport.SkipUsedSignalTag && signal.args.TryGetArg("SUBJECT", out arg) && signal.args.TryGetArg("POSITION", out LocalTargetInfo arg2) && arg != null && arg.Map == parent.Map && parent.Position.DistanceTo(arg2.Cell) <= 40f)
		{
			Trigger(arg);
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!Enabled)
		{
			return "SendSignalOnCountdownCompTime".Translate(ticksUntilEnabled.ToStringTicksToPeriod(allowSeconds: true, shortForm: false, canUseDecimals: true, allowYears: false)).Resolve();
		}
		if (!sent)
		{
			return "radius".Translate().CapitalizeFirst() + ": " + Props.radius.ToString("F0");
		}
		return "expired".Translate().CapitalizeFirst();
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref signalTag, "signalTag");
		Scribe_Values.Look(ref sent, "sent", defaultValue: false);
		Scribe_Values.Look(ref ticksUntilEnabled, "ticksUntilEnabled", 0);
	}
}
