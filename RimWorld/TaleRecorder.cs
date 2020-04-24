using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TaleRecorder
	{
		public static Tale RecordTale(TaleDef def, params object[] args)
		{
			bool flag = Rand.Value < def.ignoreChance;
			if (Rand.Value < def.ignoreChance && !DebugViewSettings.logTaleRecording)
			{
				return null;
			}
			if (def.colonistOnly)
			{
				bool flag2 = false;
				bool flag3 = false;
				for (int i = 0; i < args.Length; i++)
				{
					Pawn pawn = args[i] as Pawn;
					if (pawn != null)
					{
						flag2 = true;
						if (pawn.Faction == Faction.OfPlayer)
						{
							flag3 = true;
						}
					}
				}
				if (flag2 && !flag3)
				{
					return null;
				}
			}
			Tale tale = TaleFactory.MakeRawTale(def, args);
			if (tale == null)
			{
				return null;
			}
			if (DebugViewSettings.logTaleRecording)
			{
				Log.Message(string.Format("Tale {0} from {1}, targets {2}:\n{3}", flag ? "ignored" : "recorded", def, args.Select((object arg) => arg.ToStringSafe()).ToCommaList(), TaleTextGenerator.GenerateTextFromTale(TextGenerationPurpose.ArtDescription, tale, Rand.Int, RulePackDefOf.ArtDescription_Sculpture)));
			}
			if (flag)
			{
				return null;
			}
			Find.TaleManager.Add(tale);
			for (int j = 0; j < args.Length; j++)
			{
				Pawn pawn2 = args[j] as Pawn;
				if (pawn2 != null)
				{
					if (!pawn2.Dead && pawn2.needs.mood != null)
					{
						pawn2.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
					}
					pawn2.records.AccumulateStoryEvent(StoryEventDefOf.TaleCreated);
				}
			}
			return tale;
		}
	}
}
