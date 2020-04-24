using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public struct ShotReport
	{
		private TargetInfo target;

		private float distance;

		private List<CoverInfo> covers;

		private float coversOverallBlockChance;

		private ThingDef coveringGas;

		private float factorFromShooterAndDist;

		private float factorFromEquipment;

		private float factorFromTargetSize;

		private float factorFromWeather;

		private float forcedMissRadius;

		private ShootLine shootLine;

		private float FactorFromPosture
		{
			get
			{
				if (target.HasThing)
				{
					Pawn pawn = target.Thing as Pawn;
					if (pawn != null && distance >= 4.5f && pawn.GetPosture() != 0)
					{
						return 0.2f;
					}
				}
				return 1f;
			}
		}

		private float FactorFromExecution
		{
			get
			{
				if (target.HasThing)
				{
					Pawn pawn = target.Thing as Pawn;
					if (pawn != null && distance <= 3.9f && pawn.GetPosture() != 0)
					{
						return 7.5f;
					}
				}
				return 1f;
			}
		}

		private float FactorFromCoveringGas
		{
			get
			{
				if (coveringGas != null)
				{
					return 1f - coveringGas.gas.accuracyPenalty;
				}
				return 1f;
			}
		}

		public float AimOnTargetChance_StandardTarget
		{
			get
			{
				float num = factorFromShooterAndDist * factorFromEquipment * factorFromWeather * FactorFromCoveringGas * FactorFromExecution;
				if (num < 0.0201f)
				{
					num = 0.0201f;
				}
				return num;
			}
		}

		public float AimOnTargetChance_IgnoringPosture => AimOnTargetChance_StandardTarget * factorFromTargetSize;

		public float AimOnTargetChance => AimOnTargetChance_IgnoringPosture * FactorFromPosture;

		public float PassCoverChance => 1f - coversOverallBlockChance;

		public float TotalEstimatedHitChance => Mathf.Clamp01(AimOnTargetChance * PassCoverChance);

		public ShootLine ShootLine => shootLine;

		public static ShotReport HitReportFor(Thing caster, Verb verb, LocalTargetInfo target)
		{
			IntVec3 cell = target.Cell;
			ShotReport result = default(ShotReport);
			result.distance = (cell - caster.Position).LengthHorizontal;
			result.target = target.ToTargetInfo(caster.Map);
			result.factorFromShooterAndDist = HitFactorFromShooter(caster, result.distance);
			result.factorFromEquipment = verb.verbProps.GetHitChanceFactor(verb.EquipmentSource, result.distance);
			result.covers = CoverUtility.CalculateCoverGiverSet(target, caster.Position, caster.Map);
			result.coversOverallBlockChance = CoverUtility.CalculateOverallBlockChance(target, caster.Position, caster.Map);
			result.coveringGas = null;
			if (verb.TryFindShootLineFromTo(verb.caster.Position, target, out result.shootLine))
			{
				foreach (IntVec3 item in result.shootLine.Points())
				{
					Thing gas = item.GetGas(caster.Map);
					if (gas != null && (result.coveringGas == null || result.coveringGas.gas.accuracyPenalty < gas.def.gas.accuracyPenalty))
					{
						result.coveringGas = gas.def;
					}
				}
			}
			else
			{
				result.shootLine = new ShootLine(IntVec3.Invalid, IntVec3.Invalid);
			}
			if (!caster.Position.Roofed(caster.Map) || !target.Cell.Roofed(caster.Map))
			{
				result.factorFromWeather = caster.Map.weatherManager.CurWeatherAccuracyMultiplier;
			}
			else
			{
				result.factorFromWeather = 1f;
			}
			if (target.HasThing)
			{
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null)
				{
					result.factorFromTargetSize = pawn.BodySize;
				}
				else
				{
					result.factorFromTargetSize = target.Thing.def.fillPercent * (float)target.Thing.def.size.x * (float)target.Thing.def.size.z * 2.5f;
				}
				result.factorFromTargetSize = Mathf.Clamp(result.factorFromTargetSize, 0.5f, 2f);
			}
			else
			{
				result.factorFromTargetSize = 1f;
			}
			result.forcedMissRadius = verb.verbProps.forcedMissRadius;
			return result;
		}

		public static float HitFactorFromShooter(Thing caster, float distance)
		{
			return HitFactorFromShooter((caster is Pawn) ? caster.GetStatValue(StatDefOf.ShootingAccuracyPawn) : caster.GetStatValue(StatDefOf.ShootingAccuracyTurret), distance);
		}

		public static float HitFactorFromShooter(float accRating, float distance)
		{
			return Mathf.Max(Mathf.Pow(accRating, distance), 0.0201f);
		}

		public string GetTextReadout()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (forcedMissRadius > 0.5f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("WeaponMissRadius".Translate() + "   " + forcedMissRadius.ToString("F1"));
				stringBuilder.AppendLine("DirectHitChance".Translate() + "   " + (1f / (float)GenRadial.NumCellsInRadius(forcedMissRadius)).ToStringPercent());
			}
			else
			{
				stringBuilder.AppendLine(" " + TotalEstimatedHitChance.ToStringPercent());
				stringBuilder.AppendLine("   " + "ShootReportShooterAbility".Translate() + "  " + factorFromShooterAndDist.ToStringPercent());
				stringBuilder.AppendLine("   " + "ShootReportWeapon".Translate() + "        " + factorFromEquipment.ToStringPercent());
				if (target.HasThing && factorFromTargetSize != 1f)
				{
					stringBuilder.AppendLine("   " + "TargetSize".Translate() + "       " + factorFromTargetSize.ToStringPercent());
				}
				if (factorFromWeather < 0.99f)
				{
					stringBuilder.AppendLine("   " + "Weather".Translate() + "         " + factorFromWeather.ToStringPercent());
				}
				if (FactorFromCoveringGas < 0.99f)
				{
					stringBuilder.AppendLine("   " + coveringGas.LabelCap + "         " + FactorFromCoveringGas.ToStringPercent());
				}
				if (FactorFromPosture < 0.9999f)
				{
					stringBuilder.AppendLine("   " + "TargetProne".Translate() + "  " + FactorFromPosture.ToStringPercent());
				}
				if (FactorFromExecution != 1f)
				{
					stringBuilder.AppendLine("   " + "Execution".Translate() + "   " + FactorFromExecution.ToStringPercent());
				}
				if (PassCoverChance < 1f)
				{
					stringBuilder.AppendLine("   " + "ShootingCover".Translate() + "        " + PassCoverChance.ToStringPercent());
					for (int i = 0; i < covers.Count; i++)
					{
						CoverInfo coverInfo = covers[i];
						if (coverInfo.BlockChance > 0f)
						{
							stringBuilder.AppendLine("     " + "CoverThingBlocksPercentOfShots".Translate(coverInfo.Thing.LabelCap, coverInfo.BlockChance.ToStringPercent(), new NamedArgument(coverInfo.Thing.def, "COVER")).CapitalizeFirst());
						}
					}
				}
				else
				{
					stringBuilder.AppendLine("   (" + "NoCoverLower".Translate() + ")");
				}
			}
			return stringBuilder.ToString();
		}

		public Thing GetRandomCoverToMissInto()
		{
			if (covers.TryRandomElementByWeight((CoverInfo c) => c.BlockChance, out CoverInfo result))
			{
				return result.Thing;
			}
			return null;
		}
	}
}
