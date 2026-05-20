using System;
using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Reward_Pawn : Reward
{
	public enum ArrivalMode
	{
		WalkIn,
		DropPod
	}

	public Pawn pawn;

	public ArrivalMode arrivalMode;

	public bool detailsHidden;

	private const string RootSymbol = "root";

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			if (pawn == null)
			{
				yield break;
			}
			foreach (GenUI.AnonymousStackElement rewardStackElementsForThing in QuestPartUtility.GetRewardStackElementsForThings(Gen.YieldSingle(pawn), detailsHidden))
			{
				yield return rewardStackElementsForThing;
			}
		}
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true);
		pawn = PawnGenerator.GeneratePawn(request);
		arrivalMode = ((!Rand.Bool) ? ArrivalMode.DropPod : ArrivalMode.WalkIn);
		valueActuallyUsed = rewardValue;
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
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
			yield return questPart_GiveToCaravan;
			yield break;
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
		yield return pawnsArrive;
	}

	public override string GetDescription(RewardsGeneratorParams parms)
	{
		if (parms.giveToCaravan)
		{
			return "Reward_Pawn_Caravan".Translate(pawn);
		}
		return arrivalMode switch
		{
			ArrivalMode.WalkIn => "Reward_Pawn_WalkIn".Translate(pawn), 
			ArrivalMode.DropPod => "Reward_Pawn_DropPod".Translate(pawn), 
			_ => throw new Exception("Unknown arrival mode: " + arrivalMode), 
		};
	}

	public override string ToString()
	{
		return GetType().Name + " (" + pawn.MarketValue.ToStringMoney() + " pawn=" + pawn.ToStringSafe() + ", arrivalMode=" + arrivalMode.ToString() + ")";
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
		Scribe_Values.Look(ref arrivalMode, "arrivalMode", ArrivalMode.WalkIn);
		Scribe_Values.Look(ref detailsHidden, "detailsHidden", defaultValue: false);
	}
}
