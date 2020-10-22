using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_CryptosleepCasket : Building_Casket
	{
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
				Job job = JobMaker.MakeJob(jobDef, this);
				myPawn.jobs.TryTakeOrderedJob(job);
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
				Pawn pawn = item as Pawn;
				if (pawn != null)
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
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => typeof(Building_CryptosleepCasket).IsAssignableFrom(def.thingClass)))
			{
				Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(item), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, (Thing x) => !((Building_CryptosleepCasket)x).HasAnyContents && traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations));
				if (building_CryptosleepCasket != null)
				{
					return building_CryptosleepCasket;
				}
			}
			return null;
		}
	}
}
