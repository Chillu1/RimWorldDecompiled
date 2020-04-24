using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class ThoughtHandler : IExposable
	{
		public Pawn pawn;

		public MemoryThoughtHandler memories;

		public SituationalThoughtHandler situational;

		private static List<Thought> tmpThoughts = new List<Thought>();

		private static List<Thought> tmpTotalMoodOffsetThoughts = new List<Thought>();

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		private static List<ISocialThought> tmpTotalOpinionOffsetThoughts = new List<ISocialThought>();

		public ThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
			memories = new MemoryThoughtHandler(pawn);
			situational = new SituationalThoughtHandler(pawn);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref memories, "memories", pawn);
		}

		public void ThoughtInterval()
		{
			situational.SituationalThoughtInterval();
			memories.MemoryThoughtInterval();
		}

		public void GetAllMoodThoughts(List<Thought> outThoughts)
		{
			outThoughts.Clear();
			List<Thought_Memory> list = memories.Memories;
			for (int i = 0; i < list.Count; i++)
			{
				Thought_Memory thought_Memory = list[i];
				if (thought_Memory.MoodOffset() != 0f)
				{
					outThoughts.Add(thought_Memory);
				}
			}
			situational.AppendMoodThoughts(outThoughts);
		}

		public void GetMoodThoughts(Thought group, List<Thought> outThoughts)
		{
			GetAllMoodThoughts(outThoughts);
			for (int num = outThoughts.Count - 1; num >= 0; num--)
			{
				if (!outThoughts[num].GroupsWith(group))
				{
					outThoughts.RemoveAt(num);
				}
			}
		}

		public float MoodOffsetOfGroup(Thought group)
		{
			GetMoodThoughts(group, tmpThoughts);
			if (!tmpThoughts.Any())
			{
				return 0f;
			}
			float num = 0f;
			float num2 = 1f;
			float num3 = 0f;
			for (int i = 0; i < tmpThoughts.Count; i++)
			{
				Thought thought = tmpThoughts[i];
				num += thought.MoodOffset();
				num3 += num2;
				num2 *= thought.def.stackedEffectMultiplier;
			}
			float num4 = num / (float)tmpThoughts.Count;
			tmpThoughts.Clear();
			return num4 * num3;
		}

		public void GetDistinctMoodThoughtGroups(List<Thought> outThoughts)
		{
			GetAllMoodThoughts(outThoughts);
			for (int num = outThoughts.Count - 1; num >= 0; num--)
			{
				Thought other = outThoughts[num];
				for (int i = 0; i < num; i++)
				{
					if (outThoughts[i].GroupsWith(other))
					{
						outThoughts.RemoveAt(num);
						break;
					}
				}
			}
		}

		public float TotalMoodOffset()
		{
			GetDistinctMoodThoughtGroups(tmpTotalMoodOffsetThoughts);
			float num = 0f;
			for (int i = 0; i < tmpTotalMoodOffsetThoughts.Count; i++)
			{
				num += MoodOffsetOfGroup(tmpTotalMoodOffsetThoughts[i]);
			}
			tmpTotalMoodOffsetThoughts.Clear();
			return num;
		}

		public void GetSocialThoughts(Pawn otherPawn, List<ISocialThought> outThoughts)
		{
			outThoughts.Clear();
			List<Thought_Memory> list = memories.Memories;
			for (int i = 0; i < list.Count; i++)
			{
				ISocialThought socialThought = list[i] as ISocialThought;
				if (socialThought != null && socialThought.OtherPawn() == otherPawn)
				{
					outThoughts.Add(socialThought);
				}
			}
			situational.AppendSocialThoughts(otherPawn, outThoughts);
		}

		public void GetSocialThoughts(Pawn otherPawn, ISocialThought group, List<ISocialThought> outThoughts)
		{
			GetSocialThoughts(otherPawn, outThoughts);
			for (int num = outThoughts.Count - 1; num >= 0; num--)
			{
				if (!((Thought)outThoughts[num]).GroupsWith((Thought)group))
				{
					outThoughts.RemoveAt(num);
				}
			}
		}

		public int OpinionOffsetOfGroup(ISocialThought group, Pawn otherPawn)
		{
			GetSocialThoughts(otherPawn, group, tmpSocialThoughts);
			for (int num = tmpSocialThoughts.Count - 1; num >= 0; num--)
			{
				if (tmpSocialThoughts[num].OpinionOffset() == 0f)
				{
					tmpSocialThoughts.RemoveAt(num);
				}
			}
			if (!tmpSocialThoughts.Any())
			{
				return 0;
			}
			ThoughtDef def = ((Thought)group).def;
			if (def.IsMemory && def.stackedEffectMultiplier != 1f)
			{
				tmpSocialThoughts.Sort((ISocialThought a, ISocialThought b) => ((Thought_Memory)a).age.CompareTo(((Thought_Memory)b).age));
			}
			float num2 = 0f;
			float num3 = 1f;
			for (int i = 0; i < tmpSocialThoughts.Count; i++)
			{
				num2 += tmpSocialThoughts[i].OpinionOffset() * num3;
				num3 *= ((Thought)tmpSocialThoughts[i]).def.stackedEffectMultiplier;
			}
			tmpSocialThoughts.Clear();
			if (num2 == 0f)
			{
				return 0;
			}
			if (num2 > 0f)
			{
				return Mathf.Max(Mathf.RoundToInt(num2), 1);
			}
			return Mathf.Min(Mathf.RoundToInt(num2), -1);
		}

		public void GetDistinctSocialThoughtGroups(Pawn otherPawn, List<ISocialThought> outThoughts)
		{
			GetSocialThoughts(otherPawn, outThoughts);
			for (int num = outThoughts.Count - 1; num >= 0; num--)
			{
				ISocialThought socialThought = outThoughts[num];
				for (int i = 0; i < num; i++)
				{
					if (((Thought)outThoughts[i]).GroupsWith((Thought)socialThought))
					{
						outThoughts.RemoveAt(num);
						break;
					}
				}
			}
		}

		public int TotalOpinionOffset(Pawn otherPawn)
		{
			GetDistinctSocialThoughtGroups(otherPawn, tmpTotalOpinionOffsetThoughts);
			int num = 0;
			for (int i = 0; i < tmpTotalOpinionOffsetThoughts.Count; i++)
			{
				num += OpinionOffsetOfGroup(tmpTotalOpinionOffsetThoughts[i], otherPawn);
			}
			tmpTotalOpinionOffsetThoughts.Clear();
			return num;
		}
	}
}
