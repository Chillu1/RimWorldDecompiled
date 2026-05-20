using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class CompTargetable : CompUseEffect, ITargetingSource
{
	private Thing selectedTarget;

	private Pawn caster;

	public CompProperties_Targetable Props => (CompProperties_Targetable)props;

	protected abstract bool PlayerChoosesTarget { get; }

	public bool CasterIsPawn => true;

	public bool IsMeleeAttack => false;

	public bool Targetable => true;

	public bool MultiSelect => false;

	public bool HidePawnTooltips => false;

	public Thing Caster => parent;

	public Pawn CasterPawn => caster;

	public Verb GetVerb => null;

	public TargetingParameters targetParams => GetTargetingParameters();

	public virtual ITargetingSource DestinationSelector => null;

	public Texture2D UIIcon => null;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_References.Look(ref selectedTarget, "selectedTarget");
	}

	public override bool SelectedUseOption(Pawn p)
	{
		if (PlayerChoosesTarget)
		{
			caster = p;
			Find.Targeter.BeginTargeting(this, null, allowNonSelectedTargetingSource: true);
			return true;
		}
		selectedTarget = null;
		return false;
	}

	public override void DoEffect(Pawn usedBy)
	{
		if ((PlayerChoosesTarget && selectedTarget == null) || (selectedTarget != null && !GetTargetingParameters().CanTarget(selectedTarget)))
		{
			return;
		}
		base.DoEffect(usedBy);
		foreach (Thing target in GetTargets(selectedTarget))
		{
			foreach (CompTargetEffect comp in parent.GetComps<CompTargetEffect>())
			{
				comp.DoEffectOn(usedBy, target);
			}
			if (Props.moteOnTarget != null)
			{
				MoteMaker.MakeAttachedOverlay(target, Props.moteOnTarget, Vector3.zero);
			}
			if (Props.fleckOnTarget != null)
			{
				FleckMaker.AttachedOverlay(target, Props.fleckOnTarget, Vector3.zero);
			}
			if (Props.moteConnecting != null)
			{
				MoteMaker.MakeConnectingLine(usedBy.DrawPos, target.DrawPos, Props.moteConnecting, usedBy.Map);
			}
			if (Props.fleckConnecting != null)
			{
				FleckMaker.ConnectingLine(usedBy.DrawPos, target.DrawPos, Props.fleckConnecting, usedBy.Map);
			}
		}
		selectedTarget = null;
	}

	protected abstract TargetingParameters GetTargetingParameters();

	public abstract IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null);

	public bool CanHitTarget(LocalTargetInfo target)
	{
		return ValidateTarget(target, showMessages: false);
	}

	public void DrawHighlight(LocalTargetInfo target)
	{
		if (target.IsValid)
		{
			GenDraw.DrawTargetHighlight(target);
		}
	}

	public void OrderForceTarget(LocalTargetInfo target)
	{
		selectedTarget = target.Thing;
		if (parent.TryGetComp<CompUsable>(out var comp))
		{
			comp.TryStartUseJob(caster, target, comp.Props.ignoreOtherReservations);
		}
		caster = null;
	}

	public void OnGUI(LocalTargetInfo target)
	{
		Widgets.MouseAttachedLabel("TargetGizmoMouse".Translate());
		if (ValidateTarget(target, showMessages: false))
		{
			GenUI.DrawMouseAttachment(TexCommand.Attack);
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public virtual bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!target.HasThing)
		{
			return false;
		}
		Thing thing = target.Thing;
		if (thing is Pawn pawn)
		{
			if (Props.psychicSensitiveTargetsOnly && pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
			{
				return false;
			}
			if (Props.ignoreQuestLodgerPawns && pawn.IsQuestLodger())
			{
				return false;
			}
			if (Props.ignorePlayerFactionPawns && pawn.Faction == Faction.OfPlayer)
			{
				return false;
			}
			if (Props.cannotHaveHediff != null && pawn.health.hediffSet.HasHediff(Props.cannotHaveHediff))
			{
				if (showMessages)
				{
					Messages.Message("CannotUseDueToHediff".Translate(parent.Named("TARGETABLE"), pawn.Named("TARGET"), Props.cannotHaveHediff.label), MessageTypeDefOf.RejectInput);
				}
				return false;
			}
		}
		if (Props.fleshCorpsesOnly && thing is Corpse corpse && !corpse.InnerPawn.RaceProps.IsFlesh)
		{
			return false;
		}
		if (Props.nonDessicatedCorpsesOnly && thing is Corpse t && t.GetRotStage() == RotStage.Dessicated)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && Props.mutantFilter != null && (!(thing is Corpse corpse2) || !corpse2.InnerPawn.IsMutant || corpse2.InnerPawn.mutant.Def != Props.mutantFilter))
		{
			return false;
		}
		return true;
	}
}
