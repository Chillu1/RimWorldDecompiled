using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class Alert_Thought : Alert
	{
		protected string explanationKey;

		private static List<Thought> tmpThoughts = new List<Thought>();

		private List<Pawn> affectedPawnsResult = new List<Pawn>();

		protected abstract ThoughtDef Thought
		{
			get;
		}

		private List<Pawn> AffectedPawns
		{
			get
			{
				affectedPawnsResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (item.Dead)
					{
						Log.Error("Dead pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists:" + item);
					}
					else
					{
						if (item.needs.mood == null)
						{
							continue;
						}
						item.needs.mood.thoughts.GetAllMoodThoughts(tmpThoughts);
						try
						{
							ThoughtDef thought = Thought;
							for (int i = 0; i < tmpThoughts.Count; i++)
							{
								if (tmpThoughts[i].def == thought && !ThoughtUtility.ThoughtNullified(item, tmpThoughts[i].def))
								{
									affectedPawnsResult.Add(item);
								}
							}
						}
						finally
						{
							tmpThoughts.Clear();
						}
					}
				}
				return affectedPawnsResult;
			}
		}

		public override string GetLabel()
		{
			int count = AffectedPawns.Count;
			string label = base.GetLabel();
			if (count > 1)
			{
				return $"{label} x{count.ToString()}";
			}
			return label;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(AffectedPawns);
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn affectedPawn in AffectedPawns)
			{
				stringBuilder.AppendLine("  - " + affectedPawn.NameShortColored.Resolve());
			}
			return explanationKey.Translate(stringBuilder.ToString());
		}
	}
}
