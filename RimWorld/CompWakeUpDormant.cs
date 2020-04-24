using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class CompWakeUpDormant : ThingComp
	{
		public bool wakeUpIfColonistClose;

		private bool sentSignal;

		private CompProperties_WakeUpDormant Props => (CompProperties_WakeUpDormant)props;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			wakeUpIfColonistClose = Props.wakeUpIfAnyColonistClose;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(250))
			{
				TickRareWorker();
			}
		}

		public void TickRareWorker()
		{
			if (!parent.Spawned)
			{
				return;
			}
			if (wakeUpIfColonistClose)
			{
				int num = GenRadial.NumCellsInRadius(Props.anyColonistCloseCheckRadius);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
					if (intVec.InBounds(parent.Map) && GenSight.LineOfSight(parent.Position, intVec, parent.Map))
					{
						foreach (Thing thing in intVec.GetThingList(parent.Map))
						{
							Pawn pawn = thing as Pawn;
							if (pawn != null && pawn.IsColonist)
							{
								Activate();
								return;
							}
						}
					}
				}
			}
			if (Props.wakeUpOnThingConstructedRadius > 0f && GenClosest.ClosestThingReachable(parent.Position, parent.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.wakeUpOnThingConstructedRadius, (Thing t) => t.Faction == Faction.OfPlayer) != null)
			{
				Activate();
			}
		}

		public void Activate(bool sendSignal = true, bool silent = false)
		{
			if (sendSignal && !sentSignal)
			{
				if (!string.IsNullOrEmpty(Props.wakeUpSignalTag))
				{
					if (Props.onlyWakeUpSameFaction)
					{
						Find.SignalManager.SendSignal(new Signal(Props.wakeUpSignalTag, parent.Named("SUBJECT"), parent.Faction.Named("FACTION")));
					}
					else
					{
						Find.SignalManager.SendSignal(new Signal(Props.wakeUpSignalTag, parent.Named("SUBJECT")));
					}
				}
				if (!silent && parent.Spawned && Props.wakeUpSound != null)
				{
					Props.wakeUpSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
				}
				sentSignal = true;
			}
			parent.TryGetComp<CompCanBeDormant>()?.WakeUp();
		}

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (Props.wakeUpOnDamage && totalDamageDealt > 0f && dinfo.Def.ExternalViolenceFor(parent))
			{
				Activate();
			}
		}

		public override void Notify_SignalReceived(Signal signal)
		{
			if (!string.IsNullOrEmpty(Props.wakeUpSignalTag))
			{
				sentSignal = true;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref wakeUpIfColonistClose, "wakeUpIfColonistClose", defaultValue: false);
			Scribe_Values.Look(ref sentSignal, "sentSignal", defaultValue: false);
		}
	}
}
