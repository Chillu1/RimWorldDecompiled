using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_SightstealerArrival : QuestNode
{
	private static readonly FloatRange DaysBetweenAttacksDaysRange = new FloatRange(0.2f, 1.5f);

	private static readonly IntRange HowlDelaySecondsRange = new IntRange(4, 10);

	private static readonly FloatRange SecondAttackStrength = new FloatRange(0.35f, 0.65f);

	private const int WavesPointsThreshold = 400;

	private const string QuestTag = "Sightstealer";

	private string WaveTag => QuestGenUtility.HardcodedTargetQuestTagWithQuestID("Sightstealer") + ".wave";

	protected override bool TestRunInt(Slate slate)
	{
		return QuestGen_Get.GetMap() != null;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Sightstealers);
		float num2 = slate.Get("points", 0f);
		string firstDelayCompleteSignal = QuestGen.GenerateNewSignal("FirstDelayCompleted");
		string secondDelayCompleteSignal = QuestGen.GenerateNewSignal("SecondDelayCompleted");
		string text = QuestGenUtility.QuestTagSignal(WaveTag + "1", "AllEnemiesDefeated");
		string text2 = QuestGenUtility.QuestTagSignal(WaveTag + "2", "AllEnemiesDefeated");
		string inSignal = QuestGenUtility.QuestTagSignal(WaveTag + "3", "AllEnemiesDefeated");
		string firstHowlDelaySignal = QuestGen.GenerateNewSignal("FirstHowlDelayed");
		string secondHowlDelaySignal = QuestGen.GenerateNewSignal("SecondHowlDelayed");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");
		quest.End(QuestEndOutcome.Unknown, 0, null, inSignal2);
		List<Pawn> list;
		if (num2 < 400f)
		{
			float points = Mathf.Max(num2 * SecondAttackStrength.RandomInRange, num);
			list = GenerateSightstealers(map, points);
			quest.PawnsArrive(list, null, arrivalMode: PawnsArrivalModeDefOf.EdgeWalkInDistributed, mapParent: map.Parent, joinPlayer: false, walkInSpot: null, customLetterLabel: null, customLetterText: null, customLetterLabelRules: null, customLetterTextRules: null, isSingleReward: false, rewardDetailsHidden: false, sendStandardLetter: false);
			quest.AddPart(new QuestPart_SightstealerWave(null, map.Parent, list, WaveTag + "1"));
			quest.End(QuestEndOutcome.Success, 0, null, text);
			return;
		}
		list = GenerateSightstealers(map, num);
		quest.PawnsArrive(list, null, map.Parent, null, joinPlayer: false, null, null, null, null, null, isSingleReward: false, rewardDetailsHidden: false, sendStandardLetter: false);
		quest.AddPart(new QuestPart_SightstealerWave(null, map.Parent, list, WaveTag + "1"));
		quest.Delay(GenTicks.SecondsToTicks(HowlDelaySecondsRange.RandomInRange), delegate
		{
			quest.SignalPass(null, null, firstHowlDelaySignal);
		}, text).debugLabel = "Wave 2 howl delay";
		quest.Letter(LetterDefOf.NegativeEvent, label: "LetterLabelSightstealerHowl".Translate(), text: "LetterSightstealerHowl".Translate(), inSignal: firstHowlDelaySignal);
		quest.PlayOneShotOnCamera(SoundDefOf.Sightstealer_DistantHowl, firstHowlDelaySignal);
		quest.Delay((int)(DaysBetweenAttacksDaysRange.RandomInRange * 60000f), delegate
		{
			quest.SignalPass(null, null, firstDelayCompleteSignal);
		}, firstHowlDelaySignal).debugLabel = "Wave 2 arrival delay";
		float points2 = Mathf.Max(num2 * SecondAttackStrength.RandomInRange, num);
		list = GenerateSightstealers(map, points2, dropShard: true);
		quest.PawnsArrive(list, arrivalMode: PawnsArrivalModeDefOf.EdgeWalkInDistributed, mapParent: map.Parent, inSignal: firstDelayCompleteSignal, joinPlayer: false, walkInSpot: null, customLetterLabel: null, customLetterText: null, customLetterLabelRules: null, customLetterTextRules: null, isSingleReward: false, rewardDetailsHidden: false, sendStandardLetter: false);
		quest.AddPart(new QuestPart_SightstealerWave(firstDelayCompleteSignal, map.Parent, list, WaveTag + "2"));
		if (Rand.Bool)
		{
			quest.Delay(GenTicks.SecondsToTicks(HowlDelaySecondsRange.RandomInRange), delegate
			{
				quest.SignalPass(null, null, secondHowlDelaySignal);
			}, text2).debugLabel = "Wave 3 howl delay";
			quest.Letter(LetterDefOf.NegativeEvent, label: "LetterLabelSightstealerHowlBig".Translate(), text: "LetterSightstealerHowlBig".Translate(), inSignal: secondHowlDelaySignal);
			quest.PlayOneShotOnCamera(SoundDefOf.Sightstealer_DistantHowlLarge, secondHowlDelaySignal);
			quest.Delay((int)(DaysBetweenAttacksDaysRange.RandomInRange * 60000f), delegate
			{
				quest.SignalPass(null, null, secondDelayCompleteSignal);
			}, secondHowlDelaySignal).debugLabel = "Wave 3 arrival delay";
			float points3 = Mathf.Max(num2, num);
			list = GenerateSightstealers(map, points3, dropShard: true);
			quest.PawnsArrive(list, arrivalMode: PawnsArrivalModeDefOf.EdgeWalkInDistributed, mapParent: map.Parent, inSignal: secondDelayCompleteSignal, joinPlayer: false, walkInSpot: null, customLetterLabel: null, customLetterText: null, customLetterLabelRules: null, customLetterTextRules: null, isSingleReward: false, rewardDetailsHidden: false, sendStandardLetter: false);
			quest.AddPart(new QuestPart_SightstealerWave(secondDelayCompleteSignal, map.Parent, list, WaveTag + "3"));
			quest.End(QuestEndOutcome.Success, 0, null, inSignal);
		}
		else
		{
			quest.End(QuestEndOutcome.Success, 0, null, text2);
		}
	}

	private List<Pawn> GenerateSightstealers(Map map, float points, bool dropShard = false)
	{
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			faction = Faction.OfEntities,
			groupKind = PawnGroupKindDefOf.Sightstealers,
			points = points,
			tile = map.Tile
		}).ToList();
		foreach (Pawn item in list)
		{
			Find.WorldPawns.PassToWorld(item);
		}
		if (dropShard)
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		return list;
	}
}
