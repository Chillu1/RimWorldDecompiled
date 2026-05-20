using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class CompObelisk : ThingComp, IActivity
{
	protected Effecter obeliskWarmupEffecter;

	protected const int WarmupDelayTicks = 240;

	protected bool warmupComplete;

	private int delayedActionTick = int.MaxValue;

	protected bool activated;

	protected int lastInteractionEffectTick = -9999999;

	private Sustainer stageOneSustainer;

	private Sustainer stageTwoSustainer;

	private Sustainer stageThreeSustainer;

	private CompStudyUnlocks studyInt;

	private CompActivity activityInt;

	private static readonly SimpleCurve SustainerStudiedStageCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.1f, 1f),
		new CurvePoint(0.5f, 2f),
		new CurvePoint(1f, 3f)
	};

	protected CompProperties_Obelisk Props => (CompProperties_Obelisk)props;

	protected virtual float InteractionEffectChanceMTBDays => 1f;

	protected CompStudyUnlocks StudyUnlocks => studyInt ?? (studyInt = parent.TryGetComp<CompStudyUnlocks>());

	public CompActivity ActivityComp => activityInt ?? (activityInt = parent.TryGetComp<CompActivity>());

	public bool StudyFinished => StudyUnlocks.Completed;

	public int StudyLevel => StudyUnlocks.Progress;

	public bool Activated => activated;

	public abstract void TriggerInteractionEffect(Pawn interactor, bool triggeredByPlayer = false);

	public override void Initialize(CompProperties props)
	{
		if (!ModLister.CheckAnomaly("CompObelisk"))
		{
			parent.Destroy();
		}
		else
		{
			base.Initialize(props);
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			OnImpact();
		}
	}

	public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
	{
		if (!ActivityComp.Deactivated)
		{
			return;
		}
		foreach (Thing leaving in leavings)
		{
			leaving.Destroy();
		}
		leavings.Clear();
	}

	public override void CompTick()
	{
		SoundTick();
		if (!warmupComplete && GenTicks.TicksGame >= delayedActionTick)
		{
			warmupComplete = true;
			obeliskWarmupEffecter?.Cleanup();
			obeliskWarmupEffecter = null;
		}
		else if (activated && obeliskWarmupEffecter == null && !warmupComplete)
		{
			obeliskWarmupEffecter = EffecterDefOf.ObeliskActionWarmup.Spawn(parent, parent.MapHeld).Trigger(parent, parent);
		}
		obeliskWarmupEffecter?.EffectTick(parent, parent);
	}

	private void OnImpact()
	{
		EffecterDefOf.ImpactDustCloud.Spawn(parent.Position, parent.Map).Cleanup();
		for (int i = 0; i < Props.rubbleFilthCount; i++)
		{
			if (CellFinder.TryFindRandomCellNear(parent.OccupiedRect().RandomCell, parent.Map, 2, null, out var result))
			{
				FilthMaker.TryMakeFilth(result, parent.Map, Rand.Bool ? ThingDefOf.Filth_Dirt : ThingDefOf.Filth_RubbleRock);
			}
		}
	}

	public void Notify_InteractedTick(Pawn interactor, int delta)
	{
		if (!activated && lastInteractionEffectTick + Props.interactionEffectCooldownDays * 60000 <= Find.TickManager.TicksGame && Rand.MTBEventOccurs(InteractionEffectChanceMTBDays, 60000f, delta))
		{
			TriggerInteractionEffect(interactor);
		}
	}

	public virtual void OnActivityActivated()
	{
		activated = true;
		delayedActionTick = GenTicks.TicksGame + 240;
	}

	public void OnPassive()
	{
	}

	public bool ShouldGoPassive()
	{
		return false;
	}

	public bool CanBeSuppressed()
	{
		return true;
	}

	public bool CanActivate()
	{
		return true;
	}

	public string ActivityTooltipExtra()
	{
		return null;
	}

	private void SoundTick()
	{
		if (!parent.Spawned)
		{
			return;
		}
		if (parent.Fogged())
		{
			stageOneSustainer?.End();
			stageTwoSustainer?.End();
			stageThreeSustainer?.End();
			return;
		}
		float num = SustainerStudiedStageCurve.Evaluate(ActivityComp.ActivityLevel);
		if (num > 0f)
		{
			if (stageOneSustainer == null || stageOneSustainer.Ended)
			{
				stageOneSustainer = SoundDefOf.ObeliskAmbientStageOne.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
			}
			stageOneSustainer.info.volumeFactor = Mathf.Min(num, 1f);
			stageOneSustainer.Maintain();
		}
		else
		{
			stageOneSustainer?.End();
		}
		if (num > 1f)
		{
			if (stageTwoSustainer == null || stageTwoSustainer.Ended)
			{
				stageTwoSustainer = SoundDefOf.ObeliskAmbientStageTwo.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
			}
			stageTwoSustainer.info.volumeFactor = Mathf.Min(num - 1f, 1f);
			stageTwoSustainer.Maintain();
		}
		else
		{
			stageTwoSustainer?.End();
		}
		if (num > 2f)
		{
			if (stageThreeSustainer == null || stageThreeSustainer.Ended)
			{
				stageThreeSustainer = SoundDefOf.ObeliskAmbientStageThree.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
			}
			stageThreeSustainer.info.volumeFactor = Mathf.Min(num - 2f, 1f);
			stageThreeSustainer.Maintain();
		}
		else
		{
			stageThreeSustainer?.End();
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "DEV: Trigger interaction effect",
				action = delegate
				{
					TriggerInteractionEffect(Find.CurrentMap.mapPawns.FreeColonistsSpawned.RandomElement());
				}
			};
			if (Find.CurrentMap.mapPawns.FreeColonistsSpawned.Count == 0)
			{
				command_Action.Disable("No colonists to interact with");
			}
			yield return command_Action;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastInteractionEffectTick, "lastInteractionEffectTick", 0);
		Scribe_Values.Look(ref activated, "activated", defaultValue: false);
		Scribe_Values.Look(ref delayedActionTick, "delayedActionTick", int.MaxValue);
	}
}
