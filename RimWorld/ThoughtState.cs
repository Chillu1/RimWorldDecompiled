using UnityEngine;

namespace RimWorld
{
	public struct ThoughtState
	{
		private int stageIndex;

		private string reason;

		private const int InactiveIndex = -99999;

		public bool Active => stageIndex != -99999;

		public int StageIndex => stageIndex;

		public string Reason => reason;

		public static ThoughtState ActiveDefault => ActiveAtStage(0);

		public static ThoughtState Inactive
		{
			get
			{
				ThoughtState result = default(ThoughtState);
				result.stageIndex = -99999;
				return result;
			}
		}

		public static ThoughtState ActiveAtStage(int stageIndex)
		{
			ThoughtState result = default(ThoughtState);
			result.stageIndex = stageIndex;
			return result;
		}

		public static ThoughtState ActiveAtStage(int stageIndex, string reason)
		{
			ThoughtState result = default(ThoughtState);
			result.stageIndex = stageIndex;
			result.reason = reason;
			return result;
		}

		public static ThoughtState ActiveWithReason(string reason)
		{
			ThoughtState activeDefault = ActiveDefault;
			activeDefault.reason = reason;
			return activeDefault;
		}

		public static implicit operator ThoughtState(bool value)
		{
			if (value)
			{
				return ActiveDefault;
			}
			return Inactive;
		}

		public bool ActiveFor(ThoughtDef thoughtDef)
		{
			if (!Active)
			{
				return false;
			}
			int num = StageIndexFor(thoughtDef);
			if (num >= 0)
			{
				return thoughtDef.stages[num] != null;
			}
			return false;
		}

		public int StageIndexFor(ThoughtDef thoughtDef)
		{
			return Mathf.Min(StageIndex, thoughtDef.stages.Count - 1);
		}
	}
}
