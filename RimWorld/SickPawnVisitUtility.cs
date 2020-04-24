using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SickPawnVisitUtility
	{
		public static Pawn FindRandomSickPawn(Pawn pawn, JoyCategory maxPatientJoy)
		{
			if (!pawn.Map.mapPawns.FreeColonistsSpawned.Where((Pawn x) => CanVisit(pawn, x, maxPatientJoy)).TryRandomElementByWeight((Pawn x) => VisitChanceScore(pawn, x), out Pawn result))
			{
				return null;
			}
			return result;
		}

		public static bool CanVisit(Pawn pawn, Pawn sick, JoyCategory maxPatientJoy)
		{
			if (sick.IsColonist && !sick.Dead && pawn != sick && sick.InBed() && sick.Awake() && !sick.IsForbidden(pawn) && sick.needs.joy != null && (int)sick.needs.joy.CurCategory <= (int)maxPatientJoy && InteractionUtility.CanReceiveInteraction(sick) && !sick.needs.food.Starving && sick.needs.rest.CurLevel > 0.33f && pawn.CanReserveAndReach(sick, PathEndMode.InteractionCell, Danger.None))
			{
				return !AboutToRecover(sick);
			}
			return false;
		}

		public static Thing FindChair(Pawn forPawn, Pawn nearPawn)
		{
			Predicate<Thing> validator = delegate(Thing x)
			{
				if (!x.def.building.isSittable)
				{
					return false;
				}
				if (x.IsForbidden(forPawn))
				{
					return false;
				}
				if (!GenSight.LineOfSight(x.Position, nearPawn.Position, nearPawn.Map))
				{
					return false;
				}
				if (!forPawn.CanReserve(x))
				{
					return false;
				}
				return (!x.def.rotatable || !(GenGeo.AngleDifferenceBetween(x.Rotation.AsAngle, (nearPawn.Position - x.Position).AngleFlat) > 95f)) ? true : false;
			};
			return GenClosest.ClosestThingReachable(nearPawn.Position, nearPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(forPawn), 2.2f, validator, null, 0, 5);
		}

		private static bool AboutToRecover(Pawn pawn)
		{
			if (pawn.Downed)
			{
				return false;
			}
			if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) && !HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				return true;
			}
			if (pawn.health.hediffSet.HasImmunizableNotImmuneHediff())
			{
				return false;
			}
			float num = 0f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
				if (hediff_Injury != null && (hediff_Injury.CanHealFromTending() || hediff_Injury.CanHealNaturally() || hediff_Injury.Bleeding))
				{
					num += hediff_Injury.Severity;
				}
			}
			return num < 8f * pawn.RaceProps.baseHealthScale;
		}

		private static float VisitChanceScore(Pawn pawn, Pawn sick)
		{
			float num = GenMath.LerpDouble(-100f, 100f, 0.05f, 2f, pawn.relations.OpinionOf(sick));
			float lengthHorizontal = (pawn.Position - sick.Position).LengthHorizontal;
			float num2 = Mathf.Clamp(GenMath.LerpDouble(0f, 150f, 1f, 0.2f, lengthHorizontal), 0.2f, 1f);
			return num * num2;
		}
	}
}
