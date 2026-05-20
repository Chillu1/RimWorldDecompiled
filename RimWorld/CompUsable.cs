using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class CompUsable : ThingComp, ITargetingSource
{
	private Texture2D icon;

	private Color? iconColor;

	public CompProperties_Usable Props => (CompProperties_Usable)props;

	private Texture2D Icon
	{
		get
		{
			if (icon == null && Props.floatMenuFactionIcon != null)
			{
				icon = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.def?.FactionIcon;
			}
			return icon;
		}
	}

	private Color IconColor
	{
		get
		{
			if (!iconColor.HasValue && Props.floatMenuFactionIcon != null)
			{
				iconColor = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.Color;
			}
			return iconColor ?? Color.white;
		}
	}

	public bool CasterIsPawn => true;

	public bool IsMeleeAttack => false;

	public bool Targetable => true;

	public bool MultiSelect => false;

	public bool HidePawnTooltips => false;

	public Thing Caster => parent;

	public Pawn CasterPawn => null;

	public Verb GetVerb => null;

	public TargetingParameters targetParams => TargetingParameters.ForPawns();

	public virtual ITargetingSource DestinationSelector => null;

	public Texture2D UIIcon => null;

	protected virtual string FloatMenuOptionLabel(Pawn pawn)
	{
		return Props.useLabel.Formatted(parent);
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
	{
		if (Props.useJob == null)
		{
			yield break;
		}
		AcceptanceReport acceptanceReport = CanBeUsedBy(myPawn, Props.ignoreOtherReservations);
		FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(FloatMenuOptionLabel(myPawn), delegate
		{
			foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
			{
				if (comp.SelectedUseOption(myPawn))
				{
					return;
				}
			}
			TryStartUseJob(myPawn, GetExtraTarget(myPawn), Props.ignoreOtherReservations);
		}, priority: Props.floatMenuOptionPriority, iconTex: Icon, iconColor: IconColor), myPawn, parent);
		if (!acceptanceReport.Accepted)
		{
			floatMenuOption.Disabled = true;
			floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
		}
		yield return floatMenuOption;
	}

	public virtual LocalTargetInfo GetExtraTarget(Pawn pawn)
	{
		return LocalTargetInfo.Invalid;
	}

	public virtual void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
	{
		if (Props.useJob == null || !CanBeUsedBy(pawn, forced))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
		{
			TaggedString taggedString = comp.ConfirmMessage(pawn);
			if (!taggedString.NullOrEmpty())
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendTagged(taggedString);
			}
		}
		string text = stringBuilder.ToString();
		if (text.NullOrEmpty())
		{
			StartJob();
		}
		else
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, StartJob));
		}
		void StartJob()
		{
			if (extraTarget == pawn)
			{
				extraTarget = LocalTargetInfo.Invalid;
			}
			Job job = (extraTarget.IsValid ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent));
			job.count = 1;
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Props.showUseGizmo)
		{
			yield return new Command_Action
			{
				icon = parent.def.uiIcon,
				defaultLabel = string.Format("{0} {1}...", "UseGizmo".Translate(), parent.def.label),
				defaultDesc = "UseGizmoTooltip".Translate(parent.def.label),
				action = delegate
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					Find.Targeter.BeginTargeting(this);
				}
			};
		}
	}

	public virtual void UsedBy(Pawn p)
	{
		if (!CanBeUsedBy(p, forced: false, ignoreReserveAndReachable: true))
		{
			return;
		}
		foreach (CompUseEffect item in from x in parent.GetComps<CompUseEffect>()
			orderby x.OrderPriority descending
			select x)
		{
			try
			{
				item.DoEffect(p);
			}
			catch (Exception ex)
			{
				Log.Error("Error in CompUseEffect: " + ex);
			}
		}
		if (Props.useMessage != null)
		{
			Messages.Message(Props.useMessage.Formatted(p.Named("PAWN"), parent), parent, MessageTypeDefOf.NeutralEvent);
		}
	}

	public virtual AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
	{
		if (!p.RaceProps.IsFlesh)
		{
			return false;
		}
		if (p.IsMutant && !Props.allowedMutants.Contains(p.mutant.Def))
		{
			return false;
		}
		PlanetTile tile = p.MapHeld.Tile;
		if (tile.Valid && !Props.layerWhitelist.NullOrEmpty() && !Props.layerWhitelist.Contains(tile.LayerDef))
		{
			return "CannotPerformPlanetLayer".Translate(tile.LayerDef.gerundLabel.Named("GERUND"), tile.LayerDef.label.Named("LAYER")).Resolve();
		}
		if (tile.Valid && !Props.layerBlacklist.NullOrEmpty() && Props.layerBlacklist.Contains(tile.LayerDef))
		{
			return "CannotPerformPlanetLayer".Translate(tile.LayerDef.gerundLabel.Named("GERUND"), tile.LayerDef.label.Named("LAYER")).Resolve();
		}
		if (parent.TryGetComp<CompPowerTrader>(out var comp) && !comp.PowerOn)
		{
			return "NoPower".Translate();
		}
		if (!ignoreReserveAndReachable && !p.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
		{
			return "NoPath".Translate();
		}
		if (!ignoreReserveAndReachable && !p.CanReserve(parent, 1, -1, null, forced))
		{
			Pawn pawn = p.Map.reservationManager.FirstRespectedReserver(parent, p) ?? p.Map.physicalInteractionReservationManager.FirstReserverOf(parent);
			if (pawn != null)
			{
				return "ReservedBy".Translate(pawn.LabelShort, pawn);
			}
			return "Reserved".Translate();
		}
		if (Props.userMustHaveHediff != null && !p.health.hediffSet.HasHediff(Props.userMustHaveHediff))
		{
			return "MustHaveHediff".Translate(Props.userMustHaveHediff);
		}
		List<ThingComp> allComps = parent.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			if (allComps[i] is CompUseEffect compUseEffect)
			{
				AcceptanceReport result = compUseEffect.CanBeUsedBy(p);
				if (!result.Accepted)
				{
					return result;
				}
			}
		}
		return true;
	}

	public bool CanHitTarget(LocalTargetInfo target)
	{
		return ValidateTarget(target, showMessages: false);
	}

	public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!target.IsValid || !target.TryGetPawn(out var pawn) || !pawn.IsPlayerControlled)
		{
			return false;
		}
		AcceptanceReport acceptanceReport = CanBeUsedBy(pawn, Props.ignoreOtherReservations);
		if (!acceptanceReport.Accepted)
		{
			if (showMessages && !acceptanceReport.Reason.NullOrEmpty())
			{
				SendCannotUseMessage(pawn, acceptanceReport.Reason);
			}
			return false;
		}
		return true;
	}

	public void SendCannotUseMessage(Pawn pawn, string reason)
	{
		Messages.Message(string.Format("{0}: {1}", "CannotGenericWorkCustom".Translate(FloatMenuOptionLabel(pawn)), reason.CapitalizeFirst()), pawn, MessageTypeDefOf.RejectInput, historical: false);
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
		foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
		{
			if (comp.SelectedUseOption(target.Pawn))
			{
				return;
			}
		}
		TryStartUseJob(target.Pawn, GetExtraTarget(target.Pawn), Props.ignoreOtherReservations);
	}

	public void OnGUI(LocalTargetInfo target)
	{
		Widgets.MouseAttachedLabel("UseGizmoMouse".Translate());
		if (ValidateTarget(target, showMessages: false))
		{
			GenUI.DrawMouseAttachment(TexCommand.Attack);
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}
}
