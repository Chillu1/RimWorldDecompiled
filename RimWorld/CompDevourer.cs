using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class CompDevourer : ThingComp, IThingHolder
{
	private ThingOwner<Thing> innerContainer;

	private int ticksDigesting;

	private int ticksToDigestFully;

	private bool wasDrafted;

	public CompProperties_Devourer Props => (CompProperties_Devourer)props;

	public Thing DigestingThing
	{
		get
		{
			if (innerContainer.InnerListForReading.Count <= 0)
			{
				return null;
			}
			return innerContainer.InnerListForReading[0];
		}
	}

	public Pawn DigestingPawn
	{
		get
		{
			Thing digestingThing = DigestingThing;
			if (digestingThing == null)
			{
				return null;
			}
			if (digestingThing is Corpse corpse)
			{
				return corpse.InnerPawn;
			}
			return digestingThing as Pawn;
		}
	}

	public bool Digesting => DigestingThing != null;

	public Pawn Pawn => parent as Pawn;

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public CompDevourer()
	{
		innerContainer = new ThingOwner<Thing>(this, LookMode.Deep, removeContentsIfDestroyed: false);
	}

	public override void CompTick()
	{
		if (Digesting)
		{
			ticksDigesting++;
		}
		if (Digesting && DigestingPawn.Dead)
		{
			CompleteDigestion();
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!Digesting)
		{
			return null;
		}
		int digestionTicks = GetDigestionTicks();
		int ticksLeftThisToil = Pawn.jobs.curDriver.ticksLeftThisToil;
		int num = ((ticksLeftThisToil < 0) ? digestionTicks : ticksLeftThisToil);
		float num2 = (float)(digestionTicks - (digestionTicks - num)) / 60f;
		return Props.digestingInspector.Formatted(DigestingThing.Named("PAWN"), num2.Named("SECONDS"));
	}

	public override void Notify_Downed()
	{
		AbortDigestion(Pawn.MapHeld);
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? _ = null)
	{
		AbortDigestion(prevMap);
	}

	public void DigestJobFinished()
	{
		if (!Pawn.BeingTransportedOnGravship)
		{
			if (ticksDigesting >= ticksToDigestFully)
			{
				CompleteDigestion();
			}
			else
			{
				AbortDigestion(Pawn.MapHeld);
			}
		}
	}

	public override void PostSwapMap()
	{
		if (Digesting)
		{
			Pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.DevourerDigest), JobCondition.InterruptForced);
		}
	}

	private void AbortDigestion(Map map)
	{
		if (!Digesting)
		{
			return;
		}
		Pawn pawn = DropPawn(map);
		Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerDigestionAborted, Pawn));
		float amount = Props.timeDamageCurve.Evaluate((float)ticksDigesting / 60f);
		DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, amount, 0f, -1f, Pawn);
		dinfo.SetApplyAllDamage(value: true);
		pawn.TakeDamage(dinfo);
		if (pawn.Faction == Faction.OfPlayer)
		{
			string str = (Pawn.Dead ? Props.messageEmergedCorpse : Props.messageEmerged);
			if (!str.NullOrEmpty())
			{
				str = str.Formatted(pawn.Named("PAWN"));
				Messages.Message(str, pawn, MessageTypeDefOf.NeutralEvent);
			}
		}
		EndDigestingJob();
		Pawn.Drawer.renderer.SetAllGraphicsDirty();
		if (Pawn.Drawer.renderer.CurAnimation == AnimationDefOf.DevourerDigesting)
		{
			Pawn.Drawer.renderer.SetAnimation(null);
		}
	}

	private void CompleteDigestion()
	{
		if (Digesting)
		{
			Pawn pawn = DropPawn(Pawn.MapHeld);
			Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerDigestionCompleted, Pawn));
			DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, Props.completeDigestionDamage, 0f, -1f, Pawn);
			dinfo.SetApplyAllDamage(value: true);
			pawn.TakeDamage(dinfo);
			if (!Props.messageDigestionCompleted.NullOrEmpty() && !pawn.Dead && pawn.Faction == Faction.OfPlayer)
			{
				Messages.Message(Props.messageDigestionCompleted.Formatted(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
			}
			Pawn.Drawer.renderer.SetAllGraphicsDirty();
			if (Pawn.Drawer.renderer.CurAnimation == AnimationDefOf.DevourerDigesting)
			{
				Pawn.Drawer.renderer.SetAnimation(null);
			}
		}
	}

	public void StartDigesting(IntVec3 origin, LocalTargetInfo target)
	{
		if (target.HasThing && target.Thing is Pawn { Spawned: not false } pawn)
		{
			DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, 99f, 0f, -1f, parent);
			pawn.GetLord()?.Notify_PawnDamaged(pawn, dinfo);
			if (pawn.drafter != null)
			{
				wasDrafted = pawn.drafter.Drafted;
			}
			pawn.DeSpawn();
			ticksDigesting = 0;
			innerContainer.TryAdd(pawn);
			ticksToDigestFully = GetDigestionTicks() - 30;
			Pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.DevourerDigest), JobCondition.InterruptForced);
			if (!Props.messageDigested.NullOrEmpty() && pawn.Faction == Faction.OfPlayer)
			{
				Messages.Message(Props.messageDigested.Formatted(pawn.Named("PAWN")), Pawn, MessageTypeDefOf.NegativeEvent);
			}
			Pawn.Rotation = Rot4.FromAngleFlat((parent.Position - origin).AngleFlat);
			Pawn.Drawer.renderer.SetAllGraphicsDirty();
			if (Pawn.Drawer.renderer.CurAnimation != AnimationDefOf.DevourerDigesting)
			{
				Pawn.Drawer.renderer.SetAnimation(AnimationDefOf.DevourerDigesting);
			}
			Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerConsumeLeap, Pawn));
		}
		else
		{
			Pawn.abilities.GetAbility(AbilityDefOf.ConsumeLeap_Devourer).ResetCooldown();
		}
	}

	private Pawn DropPawn(Map map)
	{
		if (!Digesting)
		{
			return null;
		}
		if (!innerContainer.TryDrop(DigestingThing, Pawn.PositionHeld, map, ThingPlaceMode.Near, out var lastResultingThing))
		{
			if (!RCellFinder.TryFindRandomCellNearWith(Pawn.PositionHeld, (IntVec3 c) => c.Standable(map), map, out var result, 1))
			{
				Debug.LogError("Could not drop digesting pawn from devourer!");
				return null;
			}
			lastResultingThing = GenSpawn.Spawn(innerContainer.Take(DigestingThing), result, map);
		}
		if (lastResultingThing is Corpse corpse)
		{
			return corpse.InnerPawn;
		}
		Pawn pawn = (Pawn)lastResultingThing;
		pawn.stances.stunner.StunFor(60, Pawn, addBattleLog: false, showMote: false);
		if (pawn.drafter != null)
		{
			pawn.drafter.Drafted = wasDrafted;
		}
		return pawn;
	}

	public int GetDigestionTicks()
	{
		if (DigestingThing == null)
		{
			return 0;
		}
		return Mathf.CeilToInt(Props.bodySizeDigestTimeCurve.Evaluate(DigestingPawn.BodySize) * 60f);
	}

	private void EndDigestingJob()
	{
		if (!Pawn.Dead && Pawn.CurJobDef == JobDefOf.DevourerDigest && Pawn.jobs.curDriver != null && !Pawn.jobs.curDriver.ended)
		{
			Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ticksDigesting, "ticksDigesting", 0);
		Scribe_Values.Look(ref ticksToDigestFully, "ticksToDigestFully", 0);
		Scribe_Values.Look(ref wasDrafted, "wasDrafted", defaultValue: false);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && innerContainer.removeContentsIfDestroyed)
		{
			innerContainer.removeContentsIfDestroyed = false;
		}
	}
}
