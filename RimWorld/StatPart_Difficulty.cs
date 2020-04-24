using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_Difficulty : StatPart
	{
		private List<float> factorsPerDifficulty = new List<float>();

		public override void TransformValue(StatRequest req, ref float val)
		{
			val *= Multiplier(Find.Storyteller.difficulty);
		}

		public override string ExplanationPart(StatRequest req)
		{
			return "StatsReport_DifficultyMultiplier".Translate() + ": x" + Multiplier(Find.Storyteller.difficulty).ToStringPercent();
		}

		private float Multiplier(DifficultyDef d)
		{
			int num = d.difficulty;
			if (num < 0 || num > factorsPerDifficulty.Count - 1)
			{
				Log.ErrorOnce("Not enough difficulty offsets defined for StatPart_Difficulty", 3598689);
				num = Mathf.Clamp(num, 0, factorsPerDifficulty.Count - 1);
			}
			return factorsPerDifficulty[num];
		}
	}
}
