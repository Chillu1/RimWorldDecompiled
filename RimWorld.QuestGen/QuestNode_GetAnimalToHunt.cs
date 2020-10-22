using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetAnimalToHunt : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAnimalToHuntAs;

		[NoTranslate]
		public SlateRef<string> storeCountToHuntAs;

		public SlateRef<SimpleCurve> pointsToAnimalsToHuntCountCurve;

		public SlateRef<SimpleCurve> pointsToAnimalDifficultyCurve;

		public SlateRef<FloatRange?> animalsToHuntCountRandomFactorRange;

		protected override bool TestRunInt(Slate slate)
		{
			return DoWork(slate);
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate);
		}

		private bool DoWork(Slate slate)
		{
			Map map = slate.Get<Map>("map");
			if (map == null)
			{
				return false;
			}
			float x2 = slate.Get("points", 0f);
			float animalDifficultyFromPoints = pointsToAnimalDifficultyCurve.GetValue(slate).Evaluate(x2);
			if (!map.Biome.AllWildAnimals.Where((PawnKindDef x) => map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race) && map.listerThings.ThingsOfDef(x.race).Any((Thing p) => p.Faction == null)).TryRandomElementByWeight((PawnKindDef x) => AnimalCommonalityByDifficulty(x, animalDifficultyFromPoints), out var result))
			{
				return false;
			}
			int num = 0;
			for (int i = 0; i < map.mapPawns.AllPawnsSpawned.Count; i++)
			{
				Pawn pawn = map.mapPawns.AllPawnsSpawned[i];
				if (pawn.def == result.race && !pawn.IsQuestLodger() && pawn.Faction == null)
				{
					num++;
				}
			}
			SimpleCurve value = pointsToAnimalsToHuntCountCurve.GetValue(slate);
			float randomInRange = (animalsToHuntCountRandomFactorRange.GetValue(slate) ?? FloatRange.One).RandomInRange;
			int a = Mathf.RoundToInt(value.Evaluate(x2) * randomInRange);
			a = Mathf.Min(a, num);
			a = Mathf.Max(a, 1);
			slate.Set(storeAnimalToHuntAs.GetValue(slate), result.race);
			slate.Set(storeCountToHuntAs.GetValue(slate), a);
			return true;
		}

		private float AnimalCommonalityByDifficulty(PawnKindDef animalKind, float animalDifficultyFromPoints)
		{
			float num = Mathf.Abs(animalKind.GetAnimalPointsToHuntOrSlaughter() - animalDifficultyFromPoints);
			return 1f / num;
		}
	}
}
