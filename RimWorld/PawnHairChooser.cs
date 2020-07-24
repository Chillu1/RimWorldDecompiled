using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnHairChooser
	{
		public static HairDef RandomHairDefFor(Pawn pawn, FactionDef factionType)
		{
			IEnumerable<string> enumerable = HairTagsFromBackstory(pawn);
			IEnumerable<string> enumerable2 = HairTagsFromPawnKind(pawn);
			IEnumerable<string> chosen = (enumerable2.Count() > 0) ? enumerable2 : enumerable;
			if (chosen.Count() == 0)
			{
				chosen = factionType.hairTags;
			}
			return DefDatabase<HairDef>.AllDefs.Where((HairDef hair) => hair.hairTags.SharesElementWith(chosen)).RandomElementByWeight((HairDef hair) => HairChoiceLikelihoodFor(hair, pawn));
		}

		private static IEnumerable<string> HairTagsFromBackstory(Pawn pawn)
		{
			foreach (Backstory b in pawn.story.AllBackstories)
			{
				if (b.hairTags != null)
				{
					for (int i = 0; i < b.hairTags.Count; i++)
					{
						yield return b.hairTags[i];
					}
				}
			}
		}

		private static IEnumerable<string> HairTagsFromPawnKind(Pawn pawn)
		{
			if (pawn.kindDef.hairTags != null)
			{
				for (int i = 0; i < pawn.kindDef.hairTags.Count; i++)
				{
					yield return pawn.kindDef.hairTags[i];
				}
			}
		}

		private static float HairChoiceLikelihoodFor(HairDef hair, Pawn pawn)
		{
			if (pawn.gender == Gender.None)
			{
				return 100f;
			}
			if (pawn.gender == Gender.Male)
			{
				switch (hair.hairGender)
				{
				case HairGender.Female:
					return 1f;
				case HairGender.FemaleUsually:
					return 5f;
				case HairGender.MaleUsually:
					return 30f;
				case HairGender.Male:
					return 70f;
				case HairGender.Any:
					return 60f;
				}
			}
			if (pawn.gender == Gender.Female)
			{
				switch (hair.hairGender)
				{
				case HairGender.Female:
					return 70f;
				case HairGender.FemaleUsually:
					return 30f;
				case HairGender.MaleUsually:
					return 5f;
				case HairGender.Male:
					return 1f;
				case HairGender.Any:
					return 60f;
				}
			}
			Log.Error(string.Concat("Unknown hair likelihood for ", hair, " with ", pawn));
			return 0f;
		}
	}
}
