using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class CompCanBeDormant : ThingComp
	{
		public int makeTick;

		public int wokeUpTick = int.MinValue;

		public int wakeUpOnTick = int.MinValue;

		public string wakeUpSignalTag;

		public List<string> wakeUpSignalTags;

		public const string DefaultWakeUpSignal = "CompCanBeDormant.WakeUp";

		private CompProperties_CanBeDormant Props => (CompProperties_CanBeDormant)props;

		private bool WaitingToWakeUp => wakeUpOnTick != int.MinValue;

		public bool Awake
		{
			get
			{
				if (wokeUpTick != int.MinValue)
				{
					return wokeUpTick <= Find.TickManager.TicksGame;
				}
				return false;
			}
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			makeTick = GenTicks.TicksGame;
			if (!Props.startsDormant)
			{
				WakeUp();
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			wakeUpSignalTag = Props.wakeUpSignalTag;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Wake Up";
				command_Action.action = delegate
				{
					WakeUp();
				};
				yield return command_Action;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (Awake)
			{
				if (makeTick != wokeUpTick)
				{
					return Props.awakeStateLabelKey.Translate((GenTicks.TicksGame - wokeUpTick).TicksToDays().ToString("0.#"));
				}
				return null;
			}
			return Props.dormantStateLabelKey.Translate();
		}

		public void WakeUpWithDelay()
		{
			if (!Awake)
			{
				wakeUpOnTick = Find.TickManager.TicksGame + Rand.Range(60, 300);
			}
		}

		public void WakeUp()
		{
			if (Awake)
			{
				return;
			}
			wokeUpTick = GenTicks.TicksGame;
			wakeUpOnTick = int.MinValue;
			Pawn obj = parent as Pawn;
			Building building = parent as Building;
			(obj?.GetLord() ?? building?.GetLord())?.Notify_DormancyWakeup();
			if (parent.Spawned)
			{
				IAttackTarget attackTarget = parent as IAttackTarget;
				if (attackTarget != null)
				{
					parent.Map.attackTargetsCache.UpdateTarget(attackTarget);
				}
			}
		}

		public void ToSleep()
		{
			if (!Awake)
			{
				return;
			}
			wokeUpTick = int.MinValue;
			if (parent.Spawned)
			{
				IAttackTarget attackTarget = parent as IAttackTarget;
				if (attackTarget != null)
				{
					parent.Map.attackTargetsCache.UpdateTarget(attackTarget);
				}
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			if (wakeUpOnTick != int.MinValue && Find.TickManager.TicksGame >= wakeUpOnTick)
			{
				WakeUp();
			}
			TickRareWorker();
		}

		public override void CompTick()
		{
			base.CompTick();
			if (wakeUpOnTick != int.MinValue && Find.TickManager.TicksGame >= wakeUpOnTick)
			{
				WakeUp();
			}
			if (parent.IsHashIntervalTick(250))
			{
				TickRareWorker();
			}
		}

		public void TickRareWorker()
		{
			if (parent.Spawned && !Awake && !(parent is Pawn) && !parent.Position.Fogged(parent.Map))
			{
				MoteMaker.ThrowMetaIcon(parent.Position, parent.Map, ThingDefOf.Mote_SleepZ);
			}
		}

		public override void Notify_SignalReceived(Signal signal)
		{
			if (!string.IsNullOrEmpty(wakeUpSignalTag) && !Awake && (signal.tag == wakeUpSignalTag || (wakeUpSignalTags != null && wakeUpSignalTags.Contains(signal.tag))) && signal.args.TryGetArg("SUBJECT", out Thing arg) && arg != parent && arg != null && arg.Map == parent.Map && parent.Position.DistanceTo(arg.Position) <= Props.maxDistAwakenByOther && (!signal.args.TryGetArg("FACTION", out Faction arg2) || arg2 == null || arg2 == parent.Faction) && (Props.canWakeUpFogged || !parent.Fogged()) && !WaitingToWakeUp)
			{
				WakeUpWithDelay();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref wokeUpTick, "wokeUpTick", int.MinValue);
			Scribe_Values.Look(ref wakeUpOnTick, "wakeUpOnTick", int.MinValue);
			Scribe_Values.Look(ref wakeUpSignalTag, "wakeUpSignalTag");
			Scribe_Collections.Look(ref wakeUpSignalTags, "wakeUpSignalTags", LookMode.Value);
			Scribe_Values.Look(ref makeTick, "makeTick", 0);
		}
	}
}
