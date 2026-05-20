using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class UnnaturalCorpse : Corpse
{
	private UnnaturalCorpseTracker tracker;

	private bool disappear;

	private bool teleport;

	private int lastTeleport;

	private CompForbiddable compForbidInt;

	private const int MTBTeleportHours = 24;

	public CompForbiddable Forbiddable => compForbidInt ?? (compForbidInt = GetComp<CompForbiddable>());

	public UnnaturalCorpseTracker Tracker => tracker;

	public UnnaturalCorpse()
	{
		innerContainer.contentsLookMode = LookMode.Deep;
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Destroy(mode);
		tracker?.Notify_CorpseDestroyed();
	}

	public override void PostMake()
	{
		base.PostMake();
		lastTeleport = GenTicks.TicksGame;
		Forbiddable.Forbidden = true;
		this.TryGetComp<CompRottable>().disabled = true;
	}

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (disappear)
		{
			DoDisappear();
		}
		else
		{
			if (base.Destroyed || (base.ParentHolder != null && !(base.ParentHolder is Map)))
			{
				return;
			}
			if (Rand.MTBEventOccurs(24f, 2500f, delta))
			{
				teleport = true;
			}
			if (teleport && IsOutsideView() && AnomalyUtility.TryGetNearbyUnseenCell(tracker.Haunted, out var pos))
			{
				if (base.MapHeld != tracker.Haunted.MapHeld)
				{
					SkipUtility.SkipTo(this, pos, tracker.Haunted.MapHeld);
				}
				else
				{
					base.Position = pos;
				}
				teleport = false;
				lastTeleport = GenTicks.TicksGame;
				base.Map.listerHaulables.Notify_Unforbidden(this);
			}
		}
	}

	public override void TickRare()
	{
		TickRareInt();
	}

	public override void Notify_Studied(Pawn studier, float amount, KnowledgeCategoryDef category = null)
	{
		base.Notify_Studied(studier, amount, category);
		if (category.IsAnomalyResearchCategory())
		{
			tracker.Notify_Studied(studier, amount);
		}
	}

	public void DoStudiedDeactivation(Pawn interactor)
	{
		IntVec3 positionHeld = base.PositionHeld;
		Map mapHeld = base.MapHeld;
		tracker.Notify_Finished();
		EffecterDefOf.MeatExplosion.Spawn(base.PositionHeld, base.MapHeld).Cleanup();
		Destroy();
		Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Shard), positionHeld, mapHeld);
		TaggedString label = "UnnaturalCorpseDeactivatedLetter".Translate();
		TaggedString text = "UnnaturalCorpseDeactivatedLetterDesc".Translate(interactor.Named("PAWN"));
		text += "\n\n" + "UnnaturalCorpseDeactivatedLetterDescAppended".Translate(tracker.Haunted.Named("PAWN"));
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, thing);
		if (tracker.Haunted.health.hediffSet.TryGetHediff(HediffDefOf.CorpseTorment, out var hediff))
		{
			tracker.Haunted.health.RemoveHediff(hediff);
		}
		Find.Anomaly.RemoveCorpseTracker(tracker.Haunted);
	}

	public void DoDisappear()
	{
		disappear = true;
		Thing spawnedParentOrMe = base.SpawnedParentOrMe;
		if (spawnedParentOrMe is Building_Casket building_Casket)
		{
			SendDisappearedLetter();
			building_Casket.EjectContents();
			Destroy();
		}
		else if (spawnedParentOrMe != this)
		{
			SendDisappearedLetter();
			base.ParentHolder.GetDirectlyHeldThings().TryDrop(this, ThingPlaceMode.Near, 1, out var _);
			SkipUtility.SkipDeSpawn(this);
			Destroy();
		}
		else if (IsOutsideView())
		{
			SendDisappearedLetter();
			Destroy();
		}
	}

	private void SendDisappearedLetter()
	{
		TaggedString label = "LetterLabelCorpseDisappeared".Translate();
		TaggedString text = "LetterCorpseDisappeared".Translate();
		if (!tracker.Haunted.DestroyedOrNull() && !tracker.Haunted.Dead)
		{
			text += " " + "LetterCorpseDisappearedPawnAliveAppend".Translate(tracker.Haunted.Named("PAWN"));
		}
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);
	}

	public void LinkToTracker(UnnaturalCorpseTracker newTracker)
	{
		if (tracker != null)
		{
			Log.ErrorOnce("mysterious corpse " + Label + " was linked to a new tracker when it still had one assigned.", 62521241);
		}
		tracker = newTracker;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo2 in base.GetGizmos())
		{
			yield return gizmo2;
		}
		Gizmo gizmo = AnomalyUtility.OpenCodexGizmo(this);
		if (gizmo != null)
		{
			yield return gizmo;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Disappear",
			action = delegate
			{
				Find.Anomaly.DevRemoveUnnaturalCorpse(tracker.Haunted);
			}
		};
		if (!teleport)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Teleport",
				action = delegate
				{
					teleport = true;
				}
			};
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Mental break",
			action = delegate
			{
				tracker.TryTriggerMentalBreak();
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Awake",
			action = delegate
			{
				tracker.DevAwaken();
			}
		};
		if (!tracker.CanDestroyViaResearch)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Unlock deactivation",
				action = delegate
				{
					tracker.DevUnlockDeactivation();
				}
			};
		}
	}

	private bool IsOutsideView()
	{
		if (base.SpawnedOrAnyParentSpawned && base.MapHeld.reservationManager.IsReservedByAnyoneOf(this, Faction.OfPlayer))
		{
			return false;
		}
		if (Find.CurrentMap != base.MapHeld)
		{
			return true;
		}
		return !Find.CameraDriver.CurrentViewRect.ExpandedBy(1).Contains(base.PositionHeld);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref tracker, "tracker");
		Scribe_Values.Look(ref disappear, "disappears", defaultValue: false);
		Scribe_Values.Look(ref teleport, "teleport", defaultValue: false);
		Scribe_Values.Look(ref lastTeleport, "lastTeleport", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && tracker == null)
		{
			Log.Error("Failed to find corpse tracker, this should not happen. Destroying corpse.");
			Destroy();
		}
	}
}
