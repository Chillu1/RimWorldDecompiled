using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class Building_CryptosleepCasket : Building_Casket
{
	private static List<ThingDef> cachedCaskets;

	public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
	{
		if (base.TryAcceptThing(thing, allowSpecialEffects))
		{
			if (allowSpecialEffects)
			{
				SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			return true;
		}
		return false;
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
	{
		if (myPawn.IsQuestLodger())
		{
			yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
			yield break;
		}
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn))
		{
			yield return floatMenuOption;
		}
		if (innerContainer.Count != 0)
		{
			yield break;
		}
		if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
			yield break;
		}
		JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
		string label = "EnterCryptosleepCasket".Translate();
		Action action = delegate
		{
			if (ModsConfig.BiotechActive)
			{
				if (!(myPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff_PsychicBond) || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(myPawn, hediff_PsychicBond))
				{
					myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
				}
				else
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("PsychicBondDistanceWillBeActive_Cryptosleep".Translate(myPawn.Named("PAWN"), ((Pawn)hediff_PsychicBond.target).Named("BOND")), delegate
					{
						myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
					}, destructive: true));
				}
			}
			else
			{
				myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
			}
		};
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), myPawn, this);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (base.Faction == Faction.OfPlayer && innerContainer.Count > 0 && def.building.isPlayerEjectable)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.action = EjectContents;
			command_Action.defaultLabel = "CommandPodEject".Translate();
			command_Action.defaultDesc = "CommandPodEjectDesc".Translate();
			if (innerContainer.Count == 0)
			{
				command_Action.Disable("CommandPodEjectFailEmpty".Translate());
			}
			command_Action.hotKey = KeyBindingDefOf.Misc8;
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
			yield return command_Action;
		}
	}

	public override void EjectContents()
	{
		ThingDef filth_Slime = ThingDefOf.Filth_Slime;
		foreach (Thing item in (IEnumerable<Thing>)innerContainer)
		{
			if (item is Pawn pawn)
			{
				PawnComponentsUtility.AddComponentsForSpawn(pawn);
				pawn.filth.GainFilth(filth_Slime);
				if (pawn.RaceProps.IsFlesh)
				{
					pawn.health.AddHediff(HediffDefOf.CryptosleepSickness);
				}
			}
		}
		if (!base.Destroyed)
		{
			SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
		}
		base.EjectContents();
	}

	public static Building_CryptosleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
	{
		if (cachedCaskets == null)
		{
			cachedCaskets = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.IsCryptosleepCasket).ToList();
		}
		foreach (ThingDef cachedCasket in cachedCaskets)
		{
			bool queuing = KeyBindingDefOf.QueueOrder.IsDownEvent;
			Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.PositionHeld, p.MapHeld, ThingRequest.ForDef(cachedCasket), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, Validator);
			if (building_CryptosleepCasket != null)
			{
				return building_CryptosleepCasket;
			}
			bool Validator(Thing x)
			{
				if (!((Building_CryptosleepCasket)x).HasAnyContents && (!queuing || !traveler.HasReserved(x)))
				{
					return traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations);
				}
				return false;
			}
		}
		return null;
	}
}
