using System.Collections.Generic;
using LudeonTK;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public static class GrowthUtility
{
	public readonly struct GrowthTier
	{
		public readonly byte tier;

		public readonly float pointsRequirement;

		public readonly ByteRange passionGainsRange;

		public readonly byte passionChoices;

		public readonly byte traitGains;

		public readonly byte traitChoices;

		public GrowthTier(byte tier, float pointsRequirement, ByteRange passionGainsRange, byte passionChoices, byte traitGains, byte traitChoices)
		{
			this.tier = tier;
			this.pointsRequirement = pointsRequirement;
			this.passionGainsRange = passionGainsRange;
			this.passionChoices = passionChoices;
			this.traitGains = traitGains;
			this.traitChoices = traitChoices;
		}

		public int PassionGainFor(Pawn pawn)
		{
			using (new RandBlock(Gen.HashCombineInt(pawn.GetHashCode(), tier)))
			{
				return passionGainsRange.RandomInRange;
			}
		}
	}

	public static readonly GrowthTier[] GrowthTiers = new GrowthTier[9]
	{
		new GrowthTier(0, 0f, ByteRange.Zero, 0, 1, 1),
		new GrowthTier(1, 30f, ByteRange.Zero, 0, 1, 2),
		new GrowthTier(2, 55f, ByteRange.Zero, 0, 1, 3),
		new GrowthTier(3, 80f, ByteRange.Zero, 0, 1, 4),
		new GrowthTier(4, 100f, ByteRange.One, 1, 1, 4),
		new GrowthTier(5, 120f, ByteRange.One, 2, 1, 4),
		new GrowthTier(6, 135f, ByteRange.One, 3, 1, 4),
		new GrowthTier(7, 150f, new ByteRange(2, 2), 4, 1, 4),
		new GrowthTier(8, 162f, new ByteRange(3, 4), 6, 1, 6)
	};

	public static readonly int[] GrowthMomentAges = new int[3] { 7, 10, 13 };

	public static bool IsGrowthBirthday(int age)
	{
		for (int i = 0; i < GrowthMomentAges.Length; i++)
		{
			if (age == GrowthMomentAges[i])
			{
				return true;
			}
		}
		return false;
	}

	[DebugOutput("Text generation", true)]
	private static void GrowthMomentFlavor()
	{
		if (!PawnsFinder.AllMaps_FreeColonists.TryRandomElement(out var targetPawn))
		{
			Log.Error("No colonists to generate growth moment flavor for.");
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		for (int i = 0; i < GrowthTiers.Length; i++)
		{
			int tier = i;
			list.Add(new DebugMenuOption("Tier " + tier, DebugMenuOptionMode.Action, delegate
			{
				string text = "Samples for " + targetPawn.NameShortColored.Resolve() + " at tier " + tier + ":";
				for (int j = 0; j < 10; j++)
				{
					text = text + "\n  - " + GrowthFlavorForTier(targetPawn, tier);
				}
				Log.Message(text);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	public static string GrowthFlavorForTier(Pawn pawn, int growthTier)
	{
		int num = ((growthTier >= 8) ? 4 : ((growthTier >= 6) ? 3 : ((growthTier >= 4) ? 2 : ((growthTier >= 1) ? 1 : 0))));
		GrammarRequest request = default(GrammarRequest);
		request.Includes.Add(RulePackDefOf.GrowthMomentFlavor);
		request.Constants.Add("tierSection", num.ToString());
		request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN", pawn, request.Constants, addRelationInfoSymbol: false));
		return GrammarResolver.Resolve("r_root", request);
	}
}
