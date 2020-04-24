using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Reward_Pawn : Reward
	{
		public enum ArrivalMode
		{
			WalkIn,
			DropPod
		}

		public Pawn pawn;

		public ArrivalMode arrivalMode;

		private const string RootSymbol = "root";

		public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
		{
			pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceRefugee);
			arrivalMode = ((!Rand.Bool) ? ArrivalMode.DropPod : ArrivalMode.WalkIn);
			valueActuallyUsed = rewardValue;
		}

		public override void AddQuestPartsToGeneratingQuest(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
		{
			Slate slate = RimWorld.QuestGen.QuestGen.slate;
			RimWorld.QuestGen.QuestGen.AddToGeneratedPawns(pawn);
			if (!pawn.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn);
			}
			if (parms.giveToCaravan)
			{
				QuestPart_GiveToCaravan questPart_GiveToCaravan = new QuestPart_GiveToCaravan();
				questPart_GiveToCaravan.inSignal = slate.Get<string>("inSignal");
				questPart_GiveToCaravan.Things = Gen.YieldSingle(pawn);
				RimWorld.QuestGen.QuestGen.quest.AddPart(questPart_GiveToCaravan);
				return;
			}
			QuestPart_PawnsArrive pawnsArrive = new QuestPart_PawnsArrive();
			pawnsArrive.inSignal = slate.Get<string>("inSignal");
			pawnsArrive.pawns.Add(pawn);
			pawnsArrive.arrivalMode = ((arrivalMode == ArrivalMode.DropPod) ? PawnsArrivalModeDefOf.CenterDrop : PawnsArrivalModeDefOf.EdgeWalkIn);
			pawnsArrive.joinPlayer = true;
			pawnsArrive.mapParent = slate.Get<Map>("map").Parent;
			if (!customLetterLabel.NullOrEmpty() || customLetterLabelRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddTextRequest("root", delegate(string x)
				{
					pawnsArrive.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules, customLetterLabel, "root"));
			}
			if (!customLetterText.NullOrEmpty() || customLetterTextRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddTextRequest("root", delegate(string x)
				{
					pawnsArrive.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules, customLetterText, "root"));
			}
			RimWorld.QuestGen.QuestGen.quest.AddPart(pawnsArrive);
		}

		public override string GetDescription(RewardsGeneratorParams parms)
		{
			if (parms.giveToCaravan)
			{
				return "Reward_Pawn_Caravan".Translate(pawn);
			}
			switch (arrivalMode)
			{
			case ArrivalMode.WalkIn:
				return "Reward_Pawn_WalkIn".Translate(pawn);
			case ArrivalMode.DropPod:
				return "Reward_Pawn_DropPod".Translate(pawn);
			default:
				throw new Exception("Unknown arrival mode: " + arrivalMode);
			}
		}

		public override string ToString()
		{
			return GetType().Name + " (" + pawn.MarketValue.ToStringMoney() + " pawn=" + pawn.ToStringSafe() + ", arrivalMode=" + arrivalMode + ")";
		}
	}
}
