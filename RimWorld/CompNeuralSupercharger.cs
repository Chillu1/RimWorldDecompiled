using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompNeuralSupercharger : CompRechargeable
	{
		public enum AutoUseMode
		{
			NoAutoUse,
			AutoUseWithDesire,
			AutoUseForEveryone
		}

		private static Texture2D colonistOnlyCommandTex;

		public AutoUseMode autoUseMode = AutoUseMode.AutoUseWithDesire;

		public bool allowGuests;

		private Effecter effecterCharged;

		private static Texture2D ColonistOnlyCommandTex
		{
			get
			{
				if (colonistOnlyCommandTex == null)
				{
					colonistOnlyCommandTex = ContentFinder<Texture2D>.Get("UI/Gizmos/NeuralSupercharger_AllowGuests");
				}
				return colonistOnlyCommandTex;
			}
		}

		private CompProperties_NeuralSupercharger Props => (CompProperties_NeuralSupercharger)props;

		public bool CanAutoUse(Pawn pawn)
		{
			if (!allowGuests && pawn.IsQuestLodger())
			{
				return false;
			}
			switch (autoUseMode)
			{
			case AutoUseMode.NoAutoUse:
				return false;
			case AutoUseMode.AutoUseWithDesire:
				if (pawn.Ideo != null)
				{
					return pawn.Ideo.HasPrecept(PreceptDefOf.NeuralSupercharge_Preferred);
				}
				return false;
			case AutoUseMode.AutoUseForEveryone:
				return true;
			default:
				Log.Error($"Unknown auto use mode: {autoUseMode}");
				return false;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			autoUseMode = AutoUseMode.AutoUseWithDesire;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref autoUseMode, "autoUseMode", AutoUseMode.AutoUseWithDesire);
			Scribe_Values.Look(ref allowGuests, "allowGuests", defaultValue: false);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (Props.effectCharged == null)
			{
				return;
			}
			if (parent.Spawned)
			{
				if (base.Charged)
				{
					if (effecterCharged == null)
					{
						effecterCharged = Props.effectCharged.Spawn();
						effecterCharged.Trigger(parent, new TargetInfo(parent.InteractionCell, parent.Map));
					}
					effecterCharged.EffectTick(parent, new TargetInfo(parent.InteractionCell, parent.Map));
				}
				else if (effecterCharged != null)
				{
					DespawnEffecter();
				}
			}
			else
			{
				DespawnEffecter();
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!ModLister.CheckIdeology("Neural supercharger"))
			{
				parent.Destroy();
			}
			else
			{
				base.PostSpawnSetup(respawningAfterLoad);
			}
		}

		private void DespawnEffecter()
		{
			effecterCharged?.Cleanup();
			effecterCharged = null;
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (!ModLister.CheckIdeology("Neural supercharger"))
			{
				yield break;
			}
			if (!base.Charged)
			{
				yield return new FloatMenuOption(Props.jobString + " (" + "NeuralSuperchargeNotReady".Translate() + ")", null);
				yield break;
			}
			if (selPawn.CurJob != null && selPawn.CurJob.def == JobDefOf.GetNeuralSupercharge && selPawn.CurJob.targetA.Thing == parent)
			{
				yield return new FloatMenuOption(Props.jobString + " (" + "NeuralSuperchargeAlreadyGetting".Translate() + ")", null);
				yield break;
			}
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Props.jobString, delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.GetNeuralSupercharge, parent);
				selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}), selPawn, parent);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			yield return new Command_SetNeuralSuperchargerAutoUse(this);
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.defaultLabel = "CommandNeuralSuperchargerAllowGuests".Translate();
			command_Toggle.defaultDesc = "CommandNeuralSuperchargerAllowGuestsDescription".Translate();
			command_Toggle.icon = ColonistOnlyCommandTex;
			command_Toggle.isActive = () => allowGuests;
			command_Toggle.toggleAction = delegate
			{
				allowGuests = !allowGuests;
			};
			command_Toggle.activateSound = SoundDefOf.Tick_Tiny;
			yield return command_Toggle;
		}
	}
}
