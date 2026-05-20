using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class PawnCapacityUtility
{
	public abstract class CapacityImpactor
	{
		public virtual bool IsDirect => true;

		public abstract string Readable(Pawn pawn);
	}

	public class CapacityImpactorBodyPartHealth : CapacityImpactor
	{
		public BodyPartRecord bodyPart;

		public override string Readable(Pawn pawn)
		{
			return $"{bodyPart.LabelCap}: {pawn.health.hediffSet.GetPartHealth(bodyPart)} / {bodyPart.def.GetMaxHealth(pawn)}";
		}
	}

	public class CapacityImpactorCapacity : CapacityImpactor
	{
		public PawnCapacityDef capacity;

		public override bool IsDirect => false;

		public override string Readable(Pawn pawn)
		{
			return string.Format("{0}: {1}%", capacity.GetLabelFor(pawn).CapitalizeFirst(), (pawn.health.capacities.GetLevel(capacity) * 100f).ToString("F0"));
		}
	}

	public class CapacityImpactorHediff : CapacityImpactor
	{
		public Hediff hediff;

		public override string Readable(Pawn pawn)
		{
			return $"{hediff.LabelCap}";
		}
	}

	public class CapacityImpactorGene : CapacityImpactor
	{
		public Gene gene;

		public override string Readable(Pawn pawn)
		{
			return $"{gene.LabelCap}";
		}
	}

	public class CapacityImpactorPain : CapacityImpactor
	{
		public override bool IsDirect => false;

		public override string Readable(Pawn pawn)
		{
			return string.Format("{0}: {1}%", "Pain".Translate(), (pawn.health.hediffSet.PainTotal * 100f).ToString("F0"));
		}
	}

	public static bool BodyCanEverDoCapacity(BodyDef bodyDef, PawnCapacityDef capacity)
	{
		return capacity.Worker.CanHaveCapacity(bodyDef);
	}

	public static float CalculateCapacityLevel(HediffSet diffSet, PawnCapacityDef capacity, List<CapacityImpactor> impactors = null, bool forTradePrice = false)
	{
		if (capacity.zeroIfCannotBeAwake && !diffSet.pawn.health.capacities.CanBeAwake)
		{
			impactors?.Add(new CapacityImpactorCapacity
			{
				capacity = PawnCapacityDefOf.Consciousness
			});
			return 0f;
		}
		float num = capacity.Worker.CalculateCapacityLevel(diffSet, impactors);
		if (num > 0f)
		{
			float num2 = 99999f;
			float num3 = 1f;
			for (int i = 0; i < diffSet.hediffs.Count; i++)
			{
				Hediff hediff = diffSet.hediffs[i];
				if (forTradePrice && !hediff.def.priceImpact)
				{
					continue;
				}
				List<PawnCapacityModifier> capMods = hediff.CapMods;
				if (capMods == null)
				{
					continue;
				}
				for (int j = 0; j < capMods.Count; j++)
				{
					PawnCapacityModifier pawnCapacityModifier = capMods[j];
					if (pawnCapacityModifier.capacity == capacity)
					{
						num += pawnCapacityModifier.offset;
						float num4 = pawnCapacityModifier.postFactor;
						if (hediff.CurStage != null && hediff.CurStage.capacityFactorEffectMultiplier != null)
						{
							num4 = StatWorker.ScaleFactor(num4, hediff.pawn.GetStatValue(hediff.CurStage.capacityFactorEffectMultiplier));
						}
						num3 *= num4;
						float num5 = pawnCapacityModifier.EvaluateSetMax(diffSet.pawn);
						if (num5 < num2)
						{
							num2 = num5;
						}
						impactors?.Add(new CapacityImpactorHediff
						{
							hediff = hediff
						});
					}
				}
			}
			if (ModsConfig.BiotechActive && diffSet.pawn.genes != null)
			{
				for (int k = 0; k < diffSet.pawn.genes.GenesListForReading.Count; k++)
				{
					Gene gene = diffSet.pawn.genes.GenesListForReading[k];
					if (!gene.Active)
					{
						continue;
					}
					List<PawnCapacityModifier> capMods2 = gene.def.capMods;
					if (capMods2.NullOrEmpty())
					{
						continue;
					}
					for (int l = 0; l < capMods2.Count; l++)
					{
						PawnCapacityModifier pawnCapacityModifier2 = capMods2[l];
						if (pawnCapacityModifier2.capacity == capacity)
						{
							num += pawnCapacityModifier2.offset;
							num3 *= pawnCapacityModifier2.postFactor;
							float num6 = pawnCapacityModifier2.EvaluateSetMax(diffSet.pawn);
							if (num6 < num2)
							{
								num2 = num6;
							}
							impactors?.Add(new CapacityImpactorGene
							{
								gene = gene
							});
						}
					}
				}
			}
			num *= num3;
			num = Mathf.Min(num, num2);
		}
		num = Mathf.Max(num, capacity.minValue);
		return GenMath.RoundedHundredth(num);
	}

	public static float CalculatePartEfficiency(HediffSet diffSet, BodyPartRecord part, bool ignoreAddedParts = false, List<CapacityImpactor> impactors = null)
	{
		for (BodyPartRecord parent = part.parent; parent != null; parent = parent.parent)
		{
			if (diffSet.HasDirectlyAddedPartFor(parent))
			{
				Hediff_AddedPart firstHediffMatchingPart = diffSet.GetFirstHediffMatchingPart<Hediff_AddedPart>(parent);
				impactors?.Add(new CapacityImpactorHediff
				{
					hediff = firstHediffMatchingPart
				});
				return firstHediffMatchingPart.def.addedPartProps.partEfficiency;
			}
		}
		if (part.parent != null && diffSet.PartIsMissing(part.parent))
		{
			return 0f;
		}
		float num = 1f;
		if (!ignoreAddedParts)
		{
			for (int i = 0; i < diffSet.hediffs.Count; i++)
			{
				if (diffSet.hediffs[i] is Hediff_AddedPart hediff_AddedPart && hediff_AddedPart.Part == part)
				{
					num *= hediff_AddedPart.def.addedPartProps.partEfficiency;
					if (hediff_AddedPart.def.addedPartProps.partEfficiency != 1f)
					{
						impactors?.Add(new CapacityImpactorHediff
						{
							hediff = hediff_AddedPart
						});
					}
				}
			}
		}
		float b = -1f;
		float num2 = 0f;
		bool flag = false;
		for (int j = 0; j < diffSet.hediffs.Count; j++)
		{
			if (diffSet.hediffs[j].Part == part && diffSet.hediffs[j].CurStage != null && diffSet.hediffs[j].CurStage != null)
			{
				HediffStage curStage = diffSet.hediffs[j].CurStage;
				num2 += curStage.partEfficiencyOffset;
				flag |= curStage.partIgnoreMissingHP;
				if (curStage.partEfficiencyOffset != 0f && curStage.becomeVisible)
				{
					impactors?.Add(new CapacityImpactorHediff
					{
						hediff = diffSet.hediffs[j]
					});
				}
			}
		}
		if (!flag)
		{
			float num3 = diffSet.GetPartHealth(part) / part.def.GetMaxHealth(diffSet.pawn);
			if (num3 != 1f)
			{
				if (DamageWorker_AddInjury.ShouldReduceDamageToPreservePart(part))
				{
					num3 = Mathf.InverseLerp(0.1f, 1f, num3);
				}
				impactors?.Add(new CapacityImpactorBodyPartHealth
				{
					bodyPart = part
				});
				num *= num3;
			}
		}
		num += num2;
		if (num > 0.0001f)
		{
			num = Mathf.Max(num, b);
		}
		return Mathf.Max(num, 0f);
	}

	public static float CalculateImmediatePartEfficiencyAndRecord(HediffSet diffSet, BodyPartRecord part, List<CapacityImpactor> impactors = null)
	{
		if (diffSet.AncestorHasDirectlyAddedParts(part))
		{
			return 1f;
		}
		return CalculatePartEfficiency(diffSet, part, ignoreAddedParts: false, impactors);
	}

	public static float CalculateNaturalPartsAverageEfficiency(HediffSet diffSet, BodyPartGroupDef bodyPartGroup)
	{
		float num = 0f;
		int num2 = 0;
		foreach (BodyPartRecord item in from x in diffSet.GetNotMissingParts()
			where x.groups.Contains(bodyPartGroup)
			select x)
		{
			if (!diffSet.PartOrAnyAncestorHasDirectlyAddedParts(item))
			{
				num += CalculatePartEfficiency(diffSet, item);
			}
			num2++;
		}
		if (num2 == 0 || num < 0f)
		{
			return 0f;
		}
		return num / (float)num2;
	}

	public static float CalculateTagEfficiency(HediffSet diffSet, BodyPartTagDef tag, float maximum = float.MaxValue, FloatRange lerp = default(FloatRange), List<CapacityImpactor> impactors = null, float bestPartEfficiencySpecialWeight = -1f)
	{
		BodyDef body = diffSet.pawn.RaceProps.body;
		float num = 0f;
		int num2 = 0;
		float num3 = 0f;
		List<CapacityImpactor> list = null;
		foreach (BodyPartRecord item in body.GetPartsWithTag(tag))
		{
			float num4 = CalculatePartEfficiency(diffSet, item, ignoreAddedParts: false, list);
			if (impactors != null && num4 != 1f && list == null)
			{
				list = new List<CapacityImpactor>();
				CalculatePartEfficiency(diffSet, item, ignoreAddedParts: false, list);
			}
			num += num4;
			num3 = Mathf.Max(num3, num4);
			num2++;
		}
		if (num2 == 0)
		{
			return 1f;
		}
		float num5 = ((!(bestPartEfficiencySpecialWeight >= 0f) || num2 < 2) ? (num / (float)num2) : (num3 * bestPartEfficiencySpecialWeight + (num - num3) / (float)(num2 - 1) * (1f - bestPartEfficiencySpecialWeight)));
		float num6 = num5;
		if (lerp != default(FloatRange))
		{
			num6 = lerp.LerpThroughRange(num6);
		}
		num6 = Mathf.Min(num6, maximum);
		if (impactors != null && list != null && (maximum != 1f || num5 <= 1f || num6 == 1f))
		{
			impactors.AddRange(list);
		}
		return num6;
	}

	public static float CalculateLimbEfficiency(HediffSet diffSet, BodyPartTagDef limbCoreTag, BodyPartTagDef limbSegmentTag, BodyPartTagDef limbDigitTag, float appendageWeight, out float functionalPercentage, List<CapacityImpactor> impactors)
	{
		BodyDef body = diffSet.pawn.RaceProps.body;
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		foreach (BodyPartRecord item in body.GetPartsWithTag(limbCoreTag))
		{
			float num4 = CalculateImmediatePartEfficiencyAndRecord(diffSet, item, impactors);
			foreach (BodyPartRecord connectedPart in item.GetConnectedParts(limbSegmentTag))
			{
				num4 *= CalculateImmediatePartEfficiencyAndRecord(diffSet, connectedPart, impactors);
			}
			if (item.HasChildParts(limbDigitTag))
			{
				num4 = Mathf.Lerp(num4, num4 * item.GetChildParts(limbDigitTag).Average((BodyPartRecord digitPart) => CalculateImmediatePartEfficiencyAndRecord(diffSet, digitPart, impactors)), appendageWeight);
			}
			num += num4;
			num2++;
			if (num4 > 0f)
			{
				num3++;
			}
		}
		if (num2 == 0)
		{
			functionalPercentage = 0f;
			return 0f;
		}
		functionalPercentage = (float)num3 / (float)num2;
		return num / (float)num2;
	}
}
