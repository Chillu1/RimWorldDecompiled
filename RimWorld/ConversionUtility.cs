using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ConversionUtility
{
	public static TaggedString GetCertaintyReductionFactorsDescription(Pawn pawn)
	{
		TaggedString result = string.Empty;
		if (pawn.Ideo != null)
		{
			float num = 1f;
			foreach (Precept item in pawn.Ideo.PreceptsListForReading)
			{
				if (item.def.statFactors != null)
				{
					num *= item.def.statFactors.GetStatFactorFromList(StatDefOf.CertaintyLossFactor);
				}
			}
			if (num != 1f)
			{
				result = "AbilityIdeoConvertBreakdownIdeoCertaintyReduction".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO")) + ": " + num.ToStringPercent();
			}
			float num2 = Find.Storyteller.difficulty.CertaintyReductionFactor(null, pawn);
			if (num2 != 1f)
			{
				if (!result.NullOrEmpty())
				{
					result += "\n";
				}
				result += " -  " + "Difficulty_LowPopConversionBoost_Label".Translate() + ": " + num2.ToStringPercent();
			}
		}
		return result;
	}

	public static float ConversionPowerFactor_MemesVsTraits(Pawn initiator, Pawn recipient, StringBuilder sb = null)
	{
		return Mathf.Max(1f + OffsetFromIdeo(initiator, invert: false) + OffsetFromIdeo(recipient, invert: true), -0.4f);
		string MemeAndTraitDesc(MemeDef meme, Trait trait, float offset)
		{
			if (sb == null)
			{
				return string.Empty;
			}
			return "\n   -  " + "AbilityIdeoConvertBreakdownMemeVsTrait".Translate(meme.label.Named("MEME"), trait.Label.Named("TRAIT")).CapitalizeFirst() + ": " + (1f + offset).ToStringPercent();
		}
		float OffsetFromIdeo(Pawn pawn, bool invert)
		{
			Ideo ideo = pawn.Ideo;
			string text = string.Empty;
			float num = 0f;
			if (pawn.Ideo == null)
			{
				return num;
			}
			foreach (MemeDef meme in ideo.memes)
			{
				if (!meme.agreeableTraits.NullOrEmpty())
				{
					foreach (TraitRequirement agreeableTrait in meme.agreeableTraits)
					{
						if (agreeableTrait.HasTrait(recipient))
						{
							float num2 = (invert ? (-0.2f) : 0.2f);
							num += num2;
							text += MemeAndTraitDesc(meme, agreeableTrait.GetTrait(recipient), num2);
						}
					}
				}
				if (!meme.disagreeableTraits.NullOrEmpty())
				{
					foreach (TraitRequirement disagreeableTrait in meme.disagreeableTraits)
					{
						if (disagreeableTrait.HasTrait(recipient))
						{
							float num3 = (invert ? 0.2f : (-0.2f));
							num += num3;
							text += MemeAndTraitDesc(meme, disagreeableTrait.GetTrait(recipient), num3);
						}
					}
				}
			}
			if (sb != null && !text.NullOrEmpty())
			{
				sb.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownPawnIdeo".Translate(pawn.Named("PAWN")) + ": " + text);
			}
			return num;
		}
	}
}
