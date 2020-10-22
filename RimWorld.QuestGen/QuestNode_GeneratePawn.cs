using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GeneratePawn : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		[NoTranslate]
		public SlateRef<string> addToList;

		[NoTranslate]
		public SlateRef<IEnumerable<string>> addToLists;

		public SlateRef<PawnKindDef> kindDef;

		public SlateRef<Faction> faction;

		public SlateRef<bool> forbidAnyTitle;

		public SlateRef<bool> ensureNonNumericName;

		public SlateRef<IEnumerable<TraitDef>> forcedTraits;

		public SlateRef<IEnumerable<TraitDef>> prohibitedTraits;

		public SlateRef<Pawn> extraPawnForExtraRelationChance;

		public SlateRef<float> relationWithExtraPawnChanceFactor;

		public SlateRef<bool?> allowAddictions;

		public SlateRef<float> biocodeWeaponChance;

		public SlateRef<float> biocodeApparelChance;

		public SlateRef<bool> mustBeCapableOfViolence;

		private const int MinExpertSkill = 11;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			PawnGenerationRequest request = new PawnGenerationRequest(kindDef.GetValue(slate), faction.GetValue(slate), PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, allowAddictions: allowAddictions.GetValue(slate) ?? true, forcedTraits: forcedTraits.GetValue(slate), prohibitedTraits: prohibitedTraits.GetValue(slate), biocodeWeaponChance: biocodeWeaponChance.GetValue(slate), mustBeCapableOfViolence: mustBeCapableOfViolence.GetValue(slate), colonistRelationChanceFactor: 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, extraPawnForExtraRelationChance: extraPawnForExtraRelationChance.GetValue(slate), relationWithExtraPawnChanceFactor: relationWithExtraPawnChanceFactor.GetValue(slate));
			request.BiocodeApparelChance = biocodeApparelChance.GetValue(slate);
			request.ForbidAnyTitle = forbidAnyTitle.GetValue(slate);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			if (ensureNonNumericName.GetValue(slate) && (pawn.Name == null || pawn.Name.Numerical))
			{
				pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
			}
			if (storeAs.GetValue(slate) != null)
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), pawn);
			}
			if (addToList.GetValue(slate) != null)
			{
				QuestGenUtility.AddToOrMakeList(QuestGen.slate, addToList.GetValue(slate), pawn);
			}
			if (addToLists.GetValue(slate) != null)
			{
				foreach (string item in addToLists.GetValue(slate))
				{
					QuestGenUtility.AddToOrMakeList(QuestGen.slate, item, pawn);
				}
			}
			QuestGen.AddToGeneratedPawns(pawn);
			if (!pawn.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn);
			}
		}
	}
}
