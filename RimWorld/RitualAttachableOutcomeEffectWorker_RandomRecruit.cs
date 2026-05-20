using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_RandomRecruit : RitualAttachableOutcomeEffectWorker
{
	public const float RecruitChance = 0.5f;

	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		if (Rand.Chance(0.5f))
		{
			Slate slate = new Slate();
			slate.Set("map", jobRitual.Map);
			PawnKindDef villager = PawnKindDefOf.Villager;
			Ideo ideo = jobRitual.Ritual.ideo;
			slate.Set("overridePawnGenParams", new PawnGenerationRequest(villager, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, ideo));
			QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.WandererJoins, slate);
			extraOutcomeDesc = def.letterInfoText;
		}
		else
		{
			extraOutcomeDesc = null;
		}
	}
}
