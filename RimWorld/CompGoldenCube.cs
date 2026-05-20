using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompGoldenCube : CompInteractable, IRoofCollapseAlert
{
	private CompStudyUnlocks studyUnlocksInt;

	private const float EnthrallMTBDays = 12f;

	private static readonly IntRange GoldRange = new IntRange(125, 175);

	public new CompProperties_GoldenCube Props => (CompProperties_GoldenCube)props;

	public CompStudyUnlocks StudyUnlocks => studyUnlocksInt ?? (studyUnlocksInt = parent.GetComp<CompStudyUnlocks>());

	public bool Deactivatable => StudyUnlocks.Completed;

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		if (ValidateTarget(target, showMessages: false))
		{
			OrderDeactivation(target.Pawn);
		}
	}

	public override void CompTickInterval(int delta)
	{
		base.CompTickInterval(delta);
		if (parent.IsHashIntervalTick(2500, delta) && (!AnyPawnHasHediff() || Rand.MTBEventOccurs(12f, 60000f, 2500f)) && QuestUtility.TryGetIdealColonist(out var pawn, parent.MapHeld, ValidatePawn))
		{
			GiveHediff(pawn);
		}
	}

	private void GiveHediff(Pawn pawn)
	{
		pawn.health.AddHediff(HediffDefOf.CubeInterest);
		Messages.Message("MessageGoldenCubeInterest".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (Deactivatable)
		{
			AcceptanceReport acceptanceReport = CanInteract(selPawn);
			FloatMenuOption floatMenuOption = new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
			{
				OrderDeactivation(selPawn);
			});
			if (!acceptanceReport.Accepted)
			{
				floatMenuOption.Disabled = true;
				floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
			}
			yield return floatMenuOption;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Deactivatable)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	public override string CompInspectStringExtra()
	{
		return "";
	}

	protected override void OnInteracted(Pawn caster)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (Pawn item in Find.WorldPawns.AllPawnsAlive)
		{
			if (item.health.hediffSet.HasHediff(HediffDefOf.CubeInterest) && item.IsCaravanMember() && item.Faction == Faction.OfPlayer)
			{
				PawnBanishUtility.Banish(item, giveThoughts: false);
				list2.Add(item.LabelShort);
			}
			CurePawn(item);
		}
		foreach (Map map in Find.Maps)
		{
			for (int num = map.mapPawns.AllPawns.Count - 1; num >= 0; num--)
			{
				Pawn pawn = map.mapPawns.AllPawns[num];
				if (pawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))
				{
					PawnUtility.ForceEjectFromContainer(pawn);
					pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, forced: true, forceWake: false, causedByMood: false, null, transitionSilently: true);
					list.Add(pawn.LabelShort);
				}
				CurePawn(pawn);
			}
		}
		if (list.Any() || list2.Any())
		{
			TaggedString text = "CubeDeactivationBerserkDesc".Translate(caster.Named("PAWN"));
			if (list.Any())
			{
				text += "\n\n" + "CubeDeactivationBerserkPawns".Translate() + ":\n" + list.ToLineList("- ");
			}
			if (list2.Any())
			{
				text += "\n\n" + "CubeDeactivationLostPawns".Translate() + ":\n" + list2.ToLineList("- ");
			}
			Find.LetterStack.ReceiveLetter("CubeDeactivationBerserk".Translate(), text, list.Any() ? LetterDefOf.ThreatBig : LetterDefOf.NegativeEvent);
		}
		parent.Kill();
	}

	private bool AnyPawnHasHediff()
	{
		if (parent.SpawnedOrAnyParentSpawned)
		{
			foreach (Pawn allPawn in parent.MapHeld.mapPawns.AllPawns)
			{
				if (allPawn.IsColonist && allPawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))
				{
					return true;
				}
			}
		}
		Hediff hediff;
		foreach (Map map in Find.Maps)
		{
			if (map == parent.MapHeld)
			{
				continue;
			}
			foreach (Pawn allPawn2 in map.mapPawns.AllPawns)
			{
				if (allPawn2.IsColonist && allPawn2.health.hediffSet.TryGetHediff(HediffDefOf.CubeInterest, out hediff))
				{
					return true;
				}
			}
		}
		foreach (Pawn item in Find.WorldPawns.AllPawnsAlive)
		{
			if (item.IsColonist && item.health.hediffSet.TryGetHediff(HediffDefOf.CubeInterest, out hediff))
			{
				return true;
			}
		}
		return false;
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		Thing thing = ThingMaker.MakeThing(ThingDefOf.Gold);
		thing.stackCount = GoldRange.RandomInRange;
		GenPlace.TryPlaceThing(thing, parent.PositionHeld, prevMap, ThingPlaceMode.Near);
	}

	private void CurePawn(Pawn pawn)
	{
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.CubeInterest, out var hediff))
		{
			pawn.health.RemoveHediff(hediff);
		}
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.CubeWithdrawal, out var hediff2))
		{
			pawn.health.RemoveHediff(hediff2);
		}
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.CubeComa, out var hediff3))
		{
			pawn.health.RemoveHediff(hediff3);
		}
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
		if (!result.Accepted)
		{
			return result;
		}
		if (activateBy != null)
		{
			if (activateBy.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))
			{
				return "CannotDisableCube".Translate(activateBy.Named("PAWN"));
			}
			if (checkOptionalItems && !activateBy.HasReserved(ThingDefOf.Shard) && !activateBy.CanReserveAndReachableOfDef(ThingDefOf.Shard))
			{
				return "NoItemReservedOrReachable".Translate(ThingDefOf.Shard.label);
			}
		}
		else if (checkOptionalItems && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, 1))
		{
			return "NoItemReserved".Translate(ThingDefOf.Shard.label);
		}
		return true;
	}

	private void OrderDeactivation(Pawn pawn)
	{
		TaggedString text = "CubeDeactivationConfirmation".Translate();
		List<Pawn> list = Find.Maps.SelectMany((Map m) => m.mapPawns.AllPawns.Where((Pawn p) => p.Faction == Faction.OfPlayer && p.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))).ToList();
		List<Pawn> list2 = Find.WorldObjects.Caravans.SelectMany((Caravan c) => c.pawns.InnerListForReading.Where((Pawn p) => p.Faction == Faction.OfPlayer && p.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))).ToList();
		if (list.Any())
		{
			text += "\n\n" + "CubeDeactivationConfirmationPawnsBerserk".Translate() + ":\n" + list.Select((Pawn p) => p.LabelShort).ToLineList("- ");
		}
		if (list2.Any())
		{
			text += "\n\n" + "CubeDeactivationConfirmationPawnsLost".Translate() + ":\n" + list2.Select((Pawn p) => p.LabelShort).ToLineList("- ");
		}
		text += "\n\n" + "CubeDeactivationConfirmationEnd".Translate();
		Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
		{
			if (pawn.TryFindReserveAndReachableOfDef(ThingDefOf.Shard, out var thing))
			{
				Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent, thing);
				job.count = 1;
				job.playerForced = true;
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}));
	}

	private bool ValidatePawn(Pawn pawn)
	{
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest) || pawn.health.hediffSet.HasHediff(HediffDefOf.CubeComa))
		{
			return false;
		}
		if (pawn.MapHeld != parent.MapHeld)
		{
			return false;
		}
		if (parent.IsInCaravan())
		{
			return pawn.GetCaravan()?.AllThings?.Contains(parent) == true;
		}
		return true;
	}

	public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
	{
		if (RCellFinder.TryFindRandomCellNearWith(parent.Position, (IntVec3 c) => IsValidCell(c, parent.MapHeld), parent.MapHeld, out var result, 10))
		{
			SkipUtility.SkipTo(parent, result, parent.MapHeld);
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
}
