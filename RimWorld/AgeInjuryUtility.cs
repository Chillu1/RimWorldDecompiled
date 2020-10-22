using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class AgeInjuryUtility
	{
		private const int MaxPermanentInjuryAge = 100;

		private static List<Thing> emptyIngredientsList = new List<Thing>();

		public static IEnumerable<HediffGiver_Birthday> RandomHediffsToGainOnBirthday(Pawn pawn, int age)
		{
			return RandomHediffsToGainOnBirthday(pawn.def, age);
		}

		private static IEnumerable<HediffGiver_Birthday> RandomHediffsToGainOnBirthday(ThingDef raceDef, int age)
		{
			List<HediffGiverSetDef> sets = raceDef.race.hediffGiverSets;
			if (sets == null)
			{
				yield break;
			}
			for (int i = 0; i < sets.Count; i++)
			{
				List<HediffGiver> givers = sets[i].hediffGivers;
				for (int j = 0; j < givers.Count; j++)
				{
					HediffGiver_Birthday hediffGiver_Birthday = givers[j] as HediffGiver_Birthday;
					if (hediffGiver_Birthday != null)
					{
						float x = (float)age / raceDef.race.lifeExpectancy;
						if (Rand.Value < hediffGiver_Birthday.ageFractionChanceCurve.Evaluate(x))
						{
							yield return hediffGiver_Birthday;
						}
					}
				}
			}
		}

		public static void GenerateRandomOldAgeInjuries(Pawn pawn, bool tryNotToKillPawn)
		{
			float num = (pawn.RaceProps.IsMechanoid ? 2500f : pawn.RaceProps.lifeExpectancy);
			float num2 = num / 8f;
			float b = num * 1.5f;
			float chance = (pawn.RaceProps.Humanlike ? 0.15f : 0.03f);
			int num3 = 0;
			for (float num4 = num2; num4 < Mathf.Min(pawn.ageTracker.AgeBiologicalYears, b); num4 += num2)
			{
				if (Rand.Chance(chance))
				{
					num3++;
				}
			}
			for (int i = 0; i < num3; i++)
			{
				IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts()
					where x.depth == BodyPartDepth.Outside && (x.def.permanentInjuryChanceFactor != 0f || x.def.pawnGeneratorCanAmputate) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x)
					select x;
				if (!source.Any())
				{
					continue;
				}
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(RandomPermanentInjuryDamageType(bodyPartRecord.def.frostbiteVulnerability > 0f && pawn.RaceProps.ToolUser), pawn, bodyPartRecord);
				if (bodyPartRecord.def.pawnGeneratorCanAmputate && Rand.Chance(0.3f))
				{
					Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
					hediff_MissingPart.lastInjury = hediffDefFromDamage;
					hediff_MissingPart.Part = bodyPartRecord;
					hediff_MissingPart.IsFresh = false;
					if (!tryNotToKillPawn || !pawn.health.WouldDieAfterAddingHediff(hediff_MissingPart))
					{
						pawn.health.AddHediff(hediff_MissingPart, bodyPartRecord);
						if (pawn.RaceProps.Humanlike && bodyPartRecord.def == BodyPartDefOf.Leg && Rand.Chance(0.5f))
						{
							RecipeDefOf.InstallPegLeg.Worker.ApplyOnPawn(pawn, bodyPartRecord, null, emptyIngredientsList, null);
						}
					}
				}
				else if (bodyPartRecord.def.permanentInjuryChanceFactor > 0f && hediffDefFromDamage.HasComp(typeof(HediffComp_GetsPermanent)))
				{
					Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn);
					hediff_Injury.Severity = Rand.RangeInclusive(2, 6);
					hediff_Injury.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
					hediff_Injury.Part = bodyPartRecord;
					if (!tryNotToKillPawn || !pawn.health.WouldDieAfterAddingHediff(hediff_Injury))
					{
						pawn.health.AddHediff(hediff_Injury, bodyPartRecord);
					}
				}
			}
			for (int j = 1; j < pawn.ageTracker.AgeBiologicalYears; j++)
			{
				foreach (HediffGiver_Birthday item in RandomHediffsToGainOnBirthday(pawn, j))
				{
					item.TryApplyAndSimulateSeverityChange(pawn, j, tryNotToKillPawn);
					if (pawn.Dead)
					{
						break;
					}
				}
				if (pawn.Dead)
				{
					break;
				}
			}
		}

		private static DamageDef RandomPermanentInjuryDamageType(bool allowFrostbite)
		{
			return Rand.RangeInclusive(0, 3 + (allowFrostbite ? 1 : 0)) switch
			{
				0 => DamageDefOf.Bullet, 
				1 => DamageDefOf.Scratch, 
				2 => DamageDefOf.Bite, 
				3 => DamageDefOf.Stab, 
				4 => DamageDefOf.Frostbite, 
				_ => throw new Exception(), 
			};
		}

		[DebugOutput]
		public static void PermanentInjuryCalculations()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=======Theoretical injuries=========");
			for (int i = 0; i < 10; i++)
			{
				stringBuilder.AppendLine("#" + i + ":");
				List<HediffDef> list = new List<HediffDef>();
				for (int j = 0; j < 100; j++)
				{
					foreach (HediffGiver_Birthday item in RandomHediffsToGainOnBirthday(ThingDefOf.Human, j))
					{
						if (!list.Contains(item.hediff))
						{
							list.Add(item.hediff);
							stringBuilder.AppendLine("  age " + j + " - " + item.hediff);
						}
					}
				}
			}
			Log.Message(stringBuilder.ToString());
			stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=======Actual injuries=========");
			for (int k = 0; k < 200; k++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				if (pawn.ageTracker.AgeBiologicalYears >= 40)
				{
					stringBuilder.AppendLine(string.Concat(pawn.Name, " age ", pawn.ageTracker.AgeBiologicalYears));
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						stringBuilder.AppendLine(" - " + hediff);
					}
				}
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
