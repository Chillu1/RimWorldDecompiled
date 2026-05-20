using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public sealed class CompActivity : ThingComp
{
	private float activityLevelPercent;

	private ActivityState state;

	private bool triggeredNaturally;

	private bool deactivated;

	public bool suppressionEnabled;

	public float suppressIfAbove = 0.4f;

	private float lastGraphicDirtyPercent;

	private Gizmo gizmo;

	public CompProperties_Activity Props => (CompProperties_Activity)props;

	public float ActivityLevel => activityLevelPercent;

	public bool IsActive => state == ActivityState.Active;

	public bool IsDormant => state == ActivityState.Passive;

	public bool Deactivated => deactivated;

	public ActivityState State => state;

	public float ActivityResearchFactor => Props.activityResearchFactorCurve.Evaluate(ActivityLevel);

	public bool CanBeSuppressed
	{
		get
		{
			foreach (IActivity comp in parent.GetComps<IActivity>())
			{
				if (!comp.CanBeSuppressed())
				{
					return false;
				}
			}
			return true;
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		SetActivity(Props.startingRange.RandomInRange, dontDirtyGraphic: true);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.SuppressingEntities, OpportunityType.Important);
	}

	public override void CompTick()
	{
		AdjustActivity(Props.Worker.GetChangeRatePerDay(parent) / 60000f);
	}

	public override string CompInspectStringExtra()
	{
		if (Deactivated)
		{
			return "Deactivated".Translate() + ".";
		}
		if (IsActive)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		Props.Worker.GetInspectString(parent, stringBuilder);
		return stringBuilder.ToString();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Deactivated)
		{
			yield break;
		}
		if (gizmo == null)
		{
			gizmo = new ActivityGizmo(parent);
		}
		if (Find.Selector.SelectedObjects.Count == 1 && IsDormant)
		{
			yield return gizmo;
		}
		if (DebugSettings.godMode)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Activity -5%",
				action = delegate
				{
					AdjustActivity(-0.05f);
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Activity +5%",
				action = delegate
				{
					AdjustActivity(0.05f);
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: " + (IsDormant ? "Go active" : "Go passive"),
				action = delegate
				{
					activityLevelPercent = (IsDormant ? 1f : 0f);
				}
			};
		}
	}

	public void AdjustActivity(float delta)
	{
		SetActivity(activityLevelPercent + delta);
	}

	public void SetActivity(float activity, bool dontDirtyGraphic = false)
	{
		if (!Deactivated)
		{
			activityLevelPercent = Mathf.Clamp01(activity);
			if (activityLevelPercent >= 1f && state != ActivityState.Active && CanActivate())
			{
				EnterActiveState();
			}
			else if (activityLevelPercent <= 0f && state != ActivityState.Passive)
			{
				EnterPassiveState();
			}
			if (!dontDirtyGraphic && Props.dirtyGraphicsOnActivityChange && parent is Pawn pawn && Mathf.Abs(activityLevelPercent - lastGraphicDirtyPercent) > 0.01f)
			{
				lastGraphicDirtyPercent = activityLevelPercent;
				pawn.Drawer.renderer.SetAllGraphicsDirty();
			}
		}
	}

	private bool CanActivate()
	{
		foreach (IActivity comp in parent.GetComps<IActivity>())
		{
			if (!comp.CanActivate())
			{
				return false;
			}
		}
		return true;
	}

	public bool ShouldGoPassive()
	{
		if (Deactivated)
		{
			return false;
		}
		foreach (IActivity comp in parent.GetComps<IActivity>())
		{
			if (comp.ShouldGoPassive())
			{
				return true;
			}
		}
		return false;
	}

	public void EnterActiveState()
	{
		if (Deactivated)
		{
			return;
		}
		triggeredNaturally = activityLevelPercent >= 1f;
		activityLevelPercent = 1f;
		state = ActivityState.Active;
		foreach (IActivity comp in parent.GetComps<IActivity>())
		{
			comp.OnActivityActivated();
		}
		if ((triggeredNaturally || Props.showLetterOnManualActivation) && Props.showLetterOnActivated)
		{
			Find.LetterStack.ReceiveLetter(Props.letterTitle, Props.letterDesc, Props.letterDef, parent);
		}
	}

	public void EnterPassiveState()
	{
		if (Deactivated)
		{
			return;
		}
		activityLevelPercent = 0f;
		state = ActivityState.Passive;
		foreach (IActivity comp in parent.GetComps<IActivity>())
		{
			comp.OnPassive();
		}
	}

	public void Deactivate()
	{
		if (IsActive)
		{
			EnterPassiveState();
		}
		activityLevelPercent = 0f;
		deactivated = true;
		suppressionEnabled = false;
		suppressIfAbove = 0.5f;
		gizmo = null;
		parent.GetComp<CompStudiable>()?.Notify_ActivityDeactivated();
	}

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (Deactivated || !(totalDamageDealt > 0f) || !IsDormant || !(Props.changePerDamage > 0f))
		{
			return;
		}
		float x = 1f;
		if (parent is Pawn pawn)
		{
			x = pawn.health.summaryHealth.SummaryHealthPercent;
		}
		else if (parent.def.useHitPoints)
		{
			x = (float)parent.HitPoints / (float)parent.MaxHitPoints;
		}
		float num = Props.changePerDamage * totalDamageDealt * Props.damagedActivityMultiplierCurve.Evaluate(x);
		SetActivity(activityLevelPercent + num);
		if (!IsActive)
		{
			TaggedString taggedString = "MessageActivityRisingDamage".Translate(parent.LabelNoParenthesis);
			if (MessagesRepeatAvoider.MessageShowAllowed(taggedString, 15f))
			{
				Messages.Message(taggedString, parent, MessageTypeDefOf.CautionInput);
			}
		}
	}

	public void Notify_HeldOnPlatform()
	{
		suppressionEnabled = true;
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref activityLevelPercent, "activityPercent", 0f);
		Scribe_Values.Look(ref state, "state", ActivityState.Passive);
		Scribe_Values.Look(ref suppressIfAbove, "suppressIfAbove", 0f);
		Scribe_Values.Look(ref suppressionEnabled, "suppressionEnabled", defaultValue: false);
		Scribe_Values.Look(ref triggeredNaturally, "triggeredNaturally", defaultValue: false);
		Scribe_Values.Look(ref lastGraphicDirtyPercent, "lastGraphicDirtyPercent", 0f);
		Scribe_Values.Look(ref deactivated, "deactivated", defaultValue: false);
	}
}
