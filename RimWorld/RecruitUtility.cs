using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class RecruitUtility
	{
		private static readonly SimpleCurve RecruitChanceFactorCurve_Mood = new SimpleCurve
		{
			new CurvePoint(0f, 0.2f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 2f)
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_Opinion = new SimpleCurve
		{
			new CurvePoint(-100f, 0.5f),
			new CurvePoint(0f, 1f),
			new CurvePoint(100f, 2f)
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_RecruitDifficulty = new SimpleCurve
		{
			new CurvePoint(0f, 2f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 0.02f)
		};

		private const float RecruitChancePerNegotiatingAbility = 0.5f;

		public static float RecruitChanceFactorForMood(Pawn recruitee)
		{
			if (recruitee.needs.mood == null)
			{
				return 1f;
			}
			float curLevel = recruitee.needs.mood.CurLevel;
			return RecruitChanceFactorCurve_Mood.Evaluate(curLevel);
		}

		public static float RecruitChanceFactorForOpinion(Pawn recruiter, Pawn recruitee)
		{
			if (recruitee.relations == null)
			{
				return 1f;
			}
			float x = recruitee.relations.OpinionOf(recruiter);
			return RecruitChanceFactorCurve_Opinion.Evaluate(x);
		}

		public static float RecruitChanceFactorForRecruiterNegotiationAbility(Pawn recruiter)
		{
			return recruiter.GetStatValue(StatDefOf.NegotiationAbility) * 0.5f;
		}

		public static float RecruitChanceFactorForRecruiter(Pawn recruiter, Pawn recruitee)
		{
			return RecruitChanceFactorForRecruiterNegotiationAbility(recruiter) * RecruitChanceFactorForOpinion(recruiter, recruitee);
		}

		public static float RecruitChanceFactorForRecruitDifficulty(Pawn recruitee, Faction recruiterFaction)
		{
			float x = recruitee.RecruitDifficulty(recruiterFaction);
			return RecruitChanceFactorCurve_RecruitDifficulty.Evaluate(x);
		}

		public static float RecruitChanceFinalByFaction(this Pawn recruitee, Faction recruiterFaction)
		{
			return Mathf.Clamp01(RecruitChanceFactorForRecruitDifficulty(recruitee, recruiterFaction) * RecruitChanceFactorForMood(recruitee));
		}

		public static float RecruitChanceFinalByPawn(this Pawn recruitee, Pawn recruiter)
		{
			return Mathf.Clamp01(recruitee.RecruitChanceFinalByFaction(recruiter.Faction) * RecruitChanceFactorForRecruiter(recruiter, recruitee));
		}
	}
}
