using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PawnGroupKindWorker_Shamblers : PawnGroupKindWorker_Normal
{
	private static readonly FloatRange ChildrenDisabledExcludedAgeRange = new FloatRange(0f, 8f);

	protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
	{
		if (!CanGenerateFrom(parms, groupMaker))
		{
			if (errorOnZeroResults)
			{
				Log.Error("Cannot generate pawns for " + parms.faction?.ToString() + " with " + parms.points + ". Defaulting to a single random cheap group.");
			}
			return;
		}
		float num = parms.points;
		float num2 = groupMaker.options.Min((PawnGenOption opt) => opt.Cost);
		int num3 = 0;
		while (num > num2 && num3 < 200)
		{
			num3++;
			groupMaker.options.TryRandomElementByWeight((PawnGenOption gr) => gr.selectionWeight, out var result);
			if (!(result.Cost > num))
			{
				num -= result.Cost;
				DevelopmentalStage developmentalStage = DevelopmentalStage.Adult;
				if (Find.Storyteller.difficulty.ChildrenAllowed && Find.Storyteller.difficulty.childShamblersAllowed)
				{
					developmentalStage |= DevelopmentalStage.Child;
				}
				PawnKindDef kind = result.kind;
				Faction faction = parms.faction;
				DevelopmentalStage developmentalStages = developmentalStage;
				FloatRange? excludeBiologicalAgeRange = ChildrenDisabledExcludedAgeRange;
				Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, developmentalStages, null, excludeBiologicalAgeRange));
				outPawns.Add(item);
			}
		}
	}
}
