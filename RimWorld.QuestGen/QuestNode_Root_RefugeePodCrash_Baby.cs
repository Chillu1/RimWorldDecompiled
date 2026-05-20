using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_RefugeePodCrash_Baby : QuestNode_Root_WandererJoin
{
	private const float ChanceToTryGenerateParent = 0.5f;

	private const string HasParentFlagName = "hasParent";

	protected override bool TestRunInt(Slate slate)
	{
		if (Find.Storyteller.difficulty.ChildrenAllowed)
		{
			return base.TestRunInt(slate);
		}
		return false;
	}

	public override Pawn GeneratePawn()
	{
		Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: true);
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true);
		request.AllowedDevelopmentalStages = DevelopmentalStage.Baby;
		Pawn pawn = PawnGenerator.GeneratePawn(request);
		pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
		return pawn;
	}

	protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
	{
		List<Thing> list = new List<Thing> { pawn };
		if (Rand.Value < 0.5f)
		{
			Pawn mother = pawn.GetMother();
			bool flag = mother == null || Find.WorldPawns.GetSituation(mother) == WorldPawnSituation.None;
			Pawn father = pawn.GetFather();
			bool flag2 = father == null || Find.WorldPawns.GetSituation(father) == WorldPawnSituation.None;
			if (flag || flag2)
			{
				QuestGen.slate.Set("hasParent", var: true);
				PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, pawn.Faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: true, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: true);
				if (flag && !flag2)
				{
					request.FixedGender = Gender.Female;
				}
				else if (flag2 && !flag)
				{
					request.FixedGender = Gender.Male;
				}
				else if (Rand.Value < 0.5f)
				{
					request.FixedGender = Gender.Female;
				}
				else
				{
					request.FixedGender = Gender.Male;
				}
				Pawn pawn2 = PawnGenerator.GeneratePawn(request);
				pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn2);
				list.Add(pawn2.Corpse);
			}
		}
		else
		{
			QuestGen.slate.Set("hasParent", var: false);
		}
		quest.DropPods(map.Parent, list, null, null, null, null, false, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.OngoingOnly, null, destroyItemsOnCleanup: true, dropAllInSamePod: true);
	}

	[Obsolete]
	public override void SendLetter(Quest quest, Pawn pawn)
	{
		SendLetter_NewTemp(quest, pawn, Find.AnyPlayerHomeMap);
	}

	public override void SendLetter_NewTemp(Quest quest, Pawn pawn, Map map)
	{
		TaggedString title = "LetterLabelRefugeePodCrash".Translate();
		TaggedString letterText = "RefugeePodCrashBaby".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		if (QuestGen.slate.Get("hasParent", defaultValue: false))
		{
			letterText += "\n\n" + "RefugeePodCrashBabyHasParent".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		}
		QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(pawn), ref letterText);
		PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
		Find.LetterStack.ReceiveLetter(title, letterText, LetterDefOf.NeutralEvent, new TargetInfo(pawn));
	}
}
