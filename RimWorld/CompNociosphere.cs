using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompNociosphere : CompInteractable, IActivity, IRoofCollapseAlert
{
	private int nociosphereVictims;

	private int toHarm;

	public bool sentOnslaught;

	private int endOnslaughtTick;

	private int startedOnslaught;

	private IntVec3 sendLocation;

	private IntVec3 sentFromLocation;

	private int becomingUnstableTick = -1;

	private int unstableTick = -1;

	public float segScale;

	private float segScaleVel;

	private float segScaleGoal;

	private CompCauseHediff_AoE aoeComp;

	private CompPushable pushComp;

	private CompActivity activityComp;

	private CompStudyUnlocks studyUnlocks;

	private List<Thing> thingsIgnoredByExplosion;

	private static readonly FloatRange ColonistHarmRange = new FloatRange(0.4f, 0.6f);

	private static readonly FloatRange BecomingUnstableRangeDays = new FloatRange(20f, 30f);

	private const float UnstableDays = 10f;

	private static readonly int[] WarningDays = new int[2] { 2, 5 };

	private const float SmoothTime = 0.1f;

	private const float ActivityPostOnslaught = 0f;

	private const int SendableStudyIndex = 2;

	public new CompProperties_Nociosphere Props => (CompProperties_Nociosphere)props;

	public CompCauseHediff_AoE AoEComp => aoeComp ?? (aoeComp = Pawn.GetComp<CompCauseHediff_AoE>());

	public CompPushable Pushable => pushComp ?? (pushComp = Pawn.GetComp<CompPushable>());

	public CompStudyUnlocks StudyUnlocks => studyUnlocks ?? (studyUnlocks = Pawn.GetComp<CompStudyUnlocks>());

	public CompActivity Activity => activityComp ?? (activityComp = Pawn.GetComp<CompActivity>());

	public Pawn Pawn => (Pawn)parent;

	public bool CanSend
	{
		get
		{
			if (Pawn.IsOnHoldingPlatform)
			{
				return StudyUnlocks.Progress >= 2;
			}
			return false;
		}
	}

	public bool IsUnstable
	{
		get
		{
			if (unstableTick > 0)
			{
				return Find.TickManager.TicksGame > unstableTick;
			}
			return false;
		}
	}

	public bool IsBecomingUnstable
	{
		get
		{
			if (unstableTick > 0)
			{
				return Find.TickManager.TicksGame < unstableTick;
			}
			return false;
		}
	}

	public override string ExposeKey => "Nociosphere";

	public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
	{
		if (thingsIgnoredByExplosion == null)
		{
			thingsIgnoredByExplosion = new List<Thing>();
		}
		else
		{
			thingsIgnoredByExplosion.Clear();
		}
		thingsIgnoredByExplosion.AddRange(leavings);
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		IntVec3 positionHeld = Pawn.PositionHeld;
		DamageDef vaporize = DamageDefOf.Vaporize;
		Pawn pawn = Pawn;
		List<Thing> ignoredThings = thingsIgnoredByExplosion;
		GenExplosion.DoExplosion(positionHeld, prevMap, 4.9f, vaporize, pawn, -1, -1f, null, null, null, null, null, 0f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, ignoredThings);
		Find.LetterStack.ReceiveLetter("NociosphereDefeatedLabel".Translate(), "NociosphereDefeatedText".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, prevMap));
	}

	public override void CompTick()
	{
		float num = 0f;
		if (Activity.IsDormant)
		{
			segScaleGoal = 0f;
		}
		else
		{
			if (!IsUnstable && ShouldGoPassive())
			{
				Activity.EnterPassiveState();
			}
			if (unstableTick > 0)
			{
				unstableTick++;
			}
			num = 0.5f + Mathf.Sin((float)GenTicks.TicksAbs / 60f) * 0.2f;
			if (segScale >= 0.99f)
			{
				segScaleGoal = 0f;
			}
			foreach (Ability ability in Pawn.abilities.abilities)
			{
				if (ability.Casting && ability.verb.verbProps.warmupTime > 0f)
				{
					num += ability.verb.WarmupProgress * 0.7f;
					break;
				}
			}
		}
		segScale = Mathf.SmoothDamp(segScale, Mathf.Clamp01(segScaleGoal + num), ref segScaleVel, 0.1f, 10f, 1f / 60f);
		if (becomingUnstableTick <= 0)
		{
			becomingUnstableTick = Find.TickManager.TicksGame + Mathf.RoundToInt(BecomingUnstableRangeDays.RandomInRange * 60000f);
		}
		else if (Find.TickManager.TicksGame > becomingUnstableTick && Activity.IsDormant)
		{
			if (unstableTick <= 0)
			{
				unstableTick = Find.TickManager.TicksGame + Mathf.RoundToInt(600000f);
				TaggedString text = "NociosphereBecomingUnstableText".Translate();
				if (CanSend)
				{
					text += "\n\n" + "NociosphereBecomingUnstableTextExtraSendable".Translate();
				}
				else
				{
					text += "\n\n" + "NociosphereBecomingUnstableTextExtraNotSendable".Translate();
				}
				Find.LetterStack.ReceiveLetter("NociosphereBecomingUnstableLabel".Translate(), text, LetterDefOf.NeutralEvent, parent);
			}
			int[] warningDays = WarningDays;
			foreach (int num2 in warningDays)
			{
				if (unstableTick - Find.TickManager.TicksGame == 60000 * num2)
				{
					Messages.Message(Props.unstableWarning.Formatted(num2), MessageTypeDefOf.NeutralEvent);
				}
			}
		}
		if (Find.TickManager.TicksGame == unstableTick && Activity.IsDormant)
		{
			TaggedString text2 = "NociosphereUnstableText".Translate();
			if (CanSend)
			{
				text2 += "\n\n" + "NociosphereUnstableTextExtraSendable".Translate();
			}
			else
			{
				text2 += "\n\n" + "NociosphereUnstableTextExtraNotSendable".Translate();
			}
			Find.LetterStack.ReceiveLetter("NociosphereUnstableLabel".Translate(), text2, LetterDefOf.ThreatSmall, parent);
		}
		if (base.OnCooldown)
		{
			if (CanCooldown)
			{
				cooldownTicks--;
			}
			if (cooldownTicks <= 0)
			{
				CooldownEnded();
			}
		}
	}

	public override void Notify_UsedVerb(Pawn pawn, Verb verb)
	{
		if (!(verb is Verb_CastAbility verb_CastAbility) || verb_CastAbility.ability.def != AbilityDefOf.EntitySkip)
		{
			segScaleGoal = 1.2f;
		}
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Pawn.GetComp<CompActivity>().IsActive)
		{
			int num = Mathf.Max(endOnslaughtTick - GenTicks.TicksGame, 0);
			stringBuilder.Append(Props.onslaughtInspectText.Formatted(num / 60));
		}
		else if (IsBecomingUnstable)
		{
			int numTicks = Mathf.Max(unstableTick - GenTicks.TicksGame, 0);
			stringBuilder.AppendLineIfNotEmpty().Append((Props.becomingUnstableInspectText.Formatted() + ": " + numTicks.ToStringTicksToPeriod()).Colorize(ColoredText.WarningColor));
		}
		else if (IsUnstable)
		{
			stringBuilder.AppendLineIfNotEmpty().Append(Props.unstableInspectText.Colorize(ColoredText.WarningColor));
		}
		return stringBuilder.ToString();
	}

	public override void Notify_Downed()
	{
		Pawn.Kill(null, null);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!CanSend)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	public void OnActivityActivated()
	{
		if (Pawn.MapHeld == null)
		{
			return;
		}
		if (Pawn.IsOnHoldingPlatform)
		{
			Building_HoldingPlatform building_HoldingPlatform = (Building_HoldingPlatform)Pawn.ParentHolder;
			building_HoldingPlatform.innerContainer.TryDrop(Pawn, building_HoldingPlatform.Position, building_HoldingPlatform.Map, ThingPlaceMode.Direct, 1, out var _);
			CompHoldingPlatformTarget compHoldingPlatformTarget = Pawn.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget != null)
			{
				compHoldingPlatformTarget.targetHolder = null;
			}
		}
		nociosphereVictims = 0;
		Pushable.canBePushed = false;
		AoEComp.range = Mathf.Max(Pawn.MapHeld.Size.x, Pawn.MapHeld.Size.z);
		startedOnslaught = GenTicks.TicksGame;
		endOnslaughtTick = GenTicks.TicksGame + Mathf.RoundToInt(Props.sentOnslaughtDurationSeconds.RandomInRange * 60f);
		if (!sentOnslaught)
		{
			toHarm = Mathf.Max(Mathf.RoundToInt((float)Pawn.MapHeld.mapPawns.ColonistCount * ColonistHarmRange.RandomInRange), 1);
			sentFromLocation = Pawn.PositionHeld;
		}
		Pawn.MapHeld.attackTargetsCache.UpdateTarget(Pawn);
		Find.TickManager.slower.SignalForceNormalSpeed();
	}

	public void OnPassive()
	{
		if (IsBecomingUnstable || IsUnstable)
		{
			Find.LetterStack.ReceiveLetter(Props.leftLetterLabel, Props.leftLetterText, LetterDefOf.NeutralEvent);
			NociosphereUtility.SkipTo(Pawn, Pawn.Position);
			if (!Pawn.DestroyedOrNull())
			{
				Pawn.DeSpawn();
				Pawn.GetLord()?.Notify_PawnLost(Pawn, PawnLostCondition.ExitedMap);
				Find.WorldPawns.PassToWorld(Pawn, PawnDiscardDecideMode.Discard);
			}
			return;
		}
		if (CellFinder.TryFindRandomCellNear(sentFromLocation, Pawn.Map, 3, (IntVec3 c) => c.Walkable(Pawn.Map) && !c.Fogged(Pawn.Map) && c.GetFirstPawn(Pawn.Map) == null && c.GetRoom(Pawn.Map) == sentFromLocation.GetRoom(Pawn.Map), out var result))
		{
			NociosphereUtility.SkipTo(Pawn, result);
		}
		Deactivate();
		sentOnslaught = false;
		AoEComp.range = AoEComp.Props.range;
		Messages.Message(Props.onslaughtEndedMessage, Pawn, MessageTypeDefOf.NeutralEvent);
		Pawn.MapHeld.attackTargetsCache.UpdateTarget(Pawn);
		Activity.SetActivity(0f);
		StartCooldown();
	}

	public bool ShouldGoPassive()
	{
		if (!Pawn.Spawned)
		{
			return false;
		}
		if (GenTicks.TicksGame < startedOnslaught + Props.minOnslaughtTicks)
		{
			return false;
		}
		if (GenTicks.TicksGame >= endOnslaughtTick)
		{
			return true;
		}
		if (!sentOnslaught && !Pawn.Map.mapPawns.AnyColonistSpawned)
		{
			return true;
		}
		List<Pawn> list = Pawn.Map.mapPawns.FreeHumanlikesOfFaction(Faction.OfPlayer);
		if (!sentOnslaught)
		{
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].DeadOrDowned)
				{
					num++;
				}
				if (num >= toHarm)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanBeSuppressed()
	{
		return !IsUnstable;
	}

	public bool CanActivate()
	{
		return true;
	}

	public string ActivityTooltipExtra()
	{
		if (IsUnstable)
		{
			return Props.unstableInspectText.Colorize(ColoredText.WarningColor);
		}
		return "";
	}

	protected override void OnInteracted(Pawn caster)
	{
		sentOnslaught = true;
		sentFromLocation = Pawn.PositionHeld;
		NociosphereUtility.SkipTo(Pawn, sendLocation);
		Activity.EnterActiveState();
		Messages.Message("CommandSent".Translate(Pawn.Named("PAWN")), Pawn, MessageTypeDefOf.NeutralEvent);
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
		if (!result.Accepted)
		{
			return result;
		}
		if (!CanSend)
		{
			return "CannotActivateEntity".Translate();
		}
		if (!Pawn.IsOnHoldingPlatform)
		{
			return "CannotActivateEntityPlatform".Translate();
		}
		return true;
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		if (ValidateTarget(target, showMessages: false))
		{
			TargetLocation(target.Pawn);
		}
	}

	private void TargetLocation(Pawn caster)
	{
		TargetingParameters targetingParameters = TargetingParameters.ForCell();
		targetingParameters.mapBoundsContractedBy = 1;
		targetingParameters.validator = (TargetInfo c) => c.Cell.InBounds(caster.Map) && !c.Cell.Fogged(caster.Map);
		Find.Targeter.BeginTargeting(targetingParameters, delegate(LocalTargetInfo target)
		{
			sendLocation = target.Cell;
			base.OrderForceTarget(caster);
		}, delegate
		{
			Widgets.MouseAttachedLabel("ChooseNociosphereDest".Translate());
		});
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (CanSend)
		{
			AcceptanceReport acceptanceReport = CanInteract(selPawn);
			FloatMenuOption floatMenuOption = new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
			{
				TargetLocation(selPawn);
			});
			if (!acceptanceReport.Accepted)
			{
				floatMenuOption.Disabled = true;
				floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
			}
			yield return floatMenuOption;
		}
	}

	public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
	{
		if (RCellFinder.TryFindRandomCellNearWith(Pawn.Position, (IntVec3 c) => IsValidCell(c, Pawn.MapHeld), Pawn.MapHeld, out var result, 10))
		{
			NociosphereUtility.SkipTo(Pawn, result);
			Activity.AdjustActivity(Props.activityOnRoofCollapsed);
		}
		return RoofCollapseResponse.RemoveThing;
	}

	private static bool IsValidCell(IntVec3 cell, Map map)
	{
		if (cell.InBounds(map))
		{
			return cell.Walkable(map);
		}
		return false;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref nociosphereVictims, "nociosphereVictims", 0);
		Scribe_Values.Look(ref toHarm, "toHarm", 0);
		Scribe_Values.Look(ref sentOnslaught, "sentOnslaught", defaultValue: false);
		Scribe_Values.Look(ref endOnslaughtTick, "endOnslaughtTick", 0);
		Scribe_Values.Look(ref startedOnslaught, "startedOnslaught", 0);
		Scribe_Values.Look(ref sendLocation, "sendLocation");
		Scribe_Values.Look(ref sentFromLocation, "sentFromLocation");
		Scribe_Values.Look(ref becomingUnstableTick, "becomingUnstableTick", 0);
		Scribe_Values.Look(ref unstableTick, "unstableTick", 0);
		Scribe_Values.Look(ref segScale, "segScale", 0f);
		Scribe_Values.Look(ref segScaleVel, "segScaleVel", 0f);
		Scribe_Values.Look(ref segScaleGoal, "segScaleGoal", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && Pawn.IsWorldPawn())
		{
			Find.WorldPawns.RemoveAndDiscardPawnViaGC(Pawn);
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		if (!Activity.IsActive)
		{
			GenDraw.DrawRadiusRing(Pawn.PositionHeld, AoEComp.range);
		}
	}
}
