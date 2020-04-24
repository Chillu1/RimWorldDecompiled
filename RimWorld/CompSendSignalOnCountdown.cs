using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompSendSignalOnCountdown : ThingComp
	{
		public string signalTag;

		public int ticksLeft;

		private const float MaxDistActivationByOther = 40f;

		private CompProperties_SendSignalOnCountdown Props => (CompProperties_SendSignalOnCountdown)props;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			signalTag = Props.signalTag;
			ticksLeft = Mathf.CeilToInt(Rand.ByCurve(Props.countdownCurveTicks));
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Activate";
				command_Action.action = delegate
				{
					Find.SignalManager.SendSignal(new Signal(signalTag, parent.Named("SUBJECT")));
					SoundDefOf.MechanoidsWakeUp.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
					ticksLeft = 0;
				};
				yield return command_Action;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(250))
			{
				TickRareWorker();
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			TickRareWorker();
		}

		public void TickRareWorker()
		{
			if (ticksLeft > 0 && parent.Spawned)
			{
				ticksLeft -= 250;
				if (ticksLeft <= 0)
				{
					Find.SignalManager.SendSignal(new Signal(signalTag, parent.Named("SUBJECT")));
					SoundDefOf.MechanoidsWakeUp.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!parent.Spawned)
			{
				return null;
			}
			if (ticksLeft <= 0)
			{
				return "expired".Translate().CapitalizeFirst();
			}
			return "SendSignalOnCountdownCompTime".Translate(ticksLeft.ToStringTicksToPeriod());
		}

		public override void Notify_SignalReceived(Signal signal)
		{
			if (signal.tag == "CompCanBeDormant.WakeUp" && signal.args.TryGetArg("SUBJECT", out Thing arg) && arg != parent && arg != null && arg.Map == parent.Map && parent.Position.DistanceTo(arg.Position) <= 40f)
			{
				ticksLeft = 0;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
			Scribe_Values.Look(ref signalTag, "signalTag");
		}
	}
}
