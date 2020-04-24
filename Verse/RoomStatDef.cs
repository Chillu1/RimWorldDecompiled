using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class RoomStatDef : Def
	{
		public Type workerClass;

		public float updatePriority;

		public bool displayRounded;

		public bool isHidden;

		public float roomlessScore;

		public List<RoomStatScoreStage> scoreStages;

		public RoomStatDef inputStat;

		public SimpleCurve curve;

		[Unsaved(false)]
		private RoomStatWorker workerInt;

		public RoomStatWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (RoomStatWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public RoomStatScoreStage GetScoreStage(float score)
		{
			if (scoreStages.NullOrEmpty())
			{
				return null;
			}
			return scoreStages[GetScoreStageIndex(score)];
		}

		public int GetScoreStageIndex(float score)
		{
			if (scoreStages.NullOrEmpty())
			{
				throw new InvalidOperationException("No score stages available.");
			}
			int result = 0;
			for (int i = 0; i < scoreStages.Count && score >= scoreStages[i].minScore; i++)
			{
				result = i;
			}
			return result;
		}

		public string ScoreToString(float score)
		{
			if (displayRounded)
			{
				return Mathf.RoundToInt(score).ToString();
			}
			return score.ToString("F2");
		}
	}
}
