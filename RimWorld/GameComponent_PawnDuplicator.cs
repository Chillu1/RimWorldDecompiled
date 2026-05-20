using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class GameComponent_PawnDuplicator : GameComponent
	{
		private Dictionary<int, DuplicateSet> duplicates = new Dictionary<int, DuplicateSet>();

		private static readonly HashSet<int> toRemove = new HashSet<int>();

		public GameComponent_PawnDuplicator(Game game)
		{
		}

		public Pawn Duplicate(Pawn pawn)
		{
			float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
			float num = pawn.ageTracker.AgeChronologicalYearsFloat;
			if (num > ageBiologicalYearsFloat)
			{
				num = ageBiologicalYearsFloat;
			}
			PawnKindDef kindDef = pawn.kindDef;
			Faction faction = pawn.Faction;
			Gender? fixedGender = pawn.gender;
			Ideo ideo = pawn.Ideo;
			float? fixedBiologicalAge = ageBiologicalYearsFloat;
			float? fixedChronologicalAge = num;
			XenotypeDef xenotype = pawn.genes.Xenotype;
			CustomXenotype customXenotype = pawn.genes.CustomXenotype;
			PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 0f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 0f, null, null, null, null, null, fixedBiologicalAge, fixedChronologicalAge, fixedGender, null, null, null, ideo, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: true, forceDead: false, null, null, xenotype, customXenotype, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: false, dontGiveWeapon: false, onlyUseForcedBackstories: false, -1, 0, forceNoGear: true);
			request.IsCreepJoiner = pawn.IsCreepJoiner;
			request.ForceNoIdeoGear = true;
			request.CanGeneratePawnRelations = false;
			request.DontGivePreArrivalPathway = true;
			Pawn pawn2 = PawnGenerator.GeneratePawn(request);
			if (ModsConfig.AnomalyActive)
			{
				int duplicateOf = ((pawn.duplicate.duplicateOf == int.MinValue) ? pawn.thingIDNumber : pawn.duplicate.duplicateOf);
				pawn.duplicate.duplicateOf = duplicateOf;
				pawn2.duplicate.duplicateOf = duplicateOf;
			}
			pawn2.Name = NameTriple.FromString(pawn.Name.ToString());
			if (ModsConfig.BiotechActive)
			{
				pawn2.ageTracker.growthPoints = pawn.ageTracker.growthPoints;
				pawn2.ageTracker.vatGrowTicks = pawn.ageTracker.vatGrowTicks;
				pawn2.genes.xenotypeName = pawn.genes.xenotypeName;
				pawn2.genes.iconDef = pawn.genes.iconDef;
			}
			CopyStoryAndTraits(pawn, pawn2);
			CopyGenes(pawn, pawn2);
			CopyApperance(pawn, pawn2);
			CopyStyle(pawn, pawn2);
			CopySkills(pawn, pawn2);
			CopyHediffs(pawn, pawn2);
			CopyNeeds(pawn, pawn2);
			if (pawn.mutant != null)
			{
				MutantUtility.SetPawnAsMutantInstantly(pawn2, pawn.mutant.Def, pawn.GetRotStage());
			}
			CopyAbilities(pawn, pawn2);
			if (pawn.guest != null)
			{
				pawn2.guest.Recruitable = pawn.guest.Recruitable;
			}
			pawn2.Notify_DuplicatedFrom(pawn);
			pawn2.Drawer.renderer.SetAllGraphicsDirty();
			pawn2.Notify_DisabledWorkTypesChanged();
			return pawn2;
		}

		private static void CopyAbilities(Pawn pawn, Pawn newPawn)
		{
			foreach (Ability ability2 in pawn.abilities.abilities)
			{
				if (newPawn.abilities.GetAbility(ability2.def) == null)
				{
					newPawn.abilities.GainAbility(ability2.def);
				}
			}
			List<Ability> abilities = newPawn.abilities.abilities;
			for (int num = abilities.Count - 1; num >= 0; num--)
			{
				Ability ability = abilities[num];
				if (pawn.abilities.GetAbility(ability.def) == null)
				{
					newPawn.abilities.RemoveAbility(ability.def);
				}
			}
			if (pawn.royalty == null)
			{
				return;
			}
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
			{
				foreach (AbilityDef grantedAbility in item.def.grantedAbilities)
				{
					if (newPawn.abilities.GetAbility(grantedAbility) != null)
					{
						newPawn.abilities.RemoveAbility(grantedAbility);
					}
				}
			}
		}

		private static void CopyStoryAndTraits(Pawn pawn, Pawn newPawn)
		{
			newPawn.story.favoriteColor = pawn.story.favoriteColor;
			newPawn.story.Childhood = pawn.story.Childhood;
			newPawn.story.Adulthood = pawn.story.Adulthood;
			newPawn.story.traits.allTraits.Clear();
			foreach (Trait allTrait in pawn.story.traits.allTraits)
			{
				if (!ModsConfig.BiotechActive || allTrait.sourceGene == null)
				{
					newPawn.story.traits.GainTrait(new Trait(allTrait.def, allTrait.Degree, allTrait.ScenForced));
				}
			}
		}

		private static void CopyGenes(Pawn pawn, Pawn newPawn)
		{
			if (ModsConfig.BiotechActive)
			{
				newPawn.genes.Xenogenes.Clear();
				List<Gene> sourceXenogenes = pawn.genes.Xenogenes;
				foreach (Gene item in sourceXenogenes)
				{
					newPawn.genes.AddGene(item.def, xenogene: true);
				}
				int i;
				for (i = 0; i < sourceXenogenes.Count; i++)
				{
					Gene gene = newPawn.genes.Xenogenes[i];
					if (sourceXenogenes[i].Overridden)
					{
						gene.overriddenByGene = newPawn.genes.GenesListForReading.First((Gene e) => e.def == sourceXenogenes[i].overriddenByGene.def);
					}
					else
					{
						gene.overriddenByGene = null;
					}
				}
			}
			newPawn.genes.Endogenes.Clear();
			List<Gene> sourceEndogenes = pawn.genes.Endogenes;
			foreach (Gene item2 in sourceEndogenes)
			{
				newPawn.genes.AddGene(item2.def, xenogene: false);
			}
			int i2;
			for (i2 = 0; i2 < sourceEndogenes.Count; i2++)
			{
				Gene gene2 = newPawn.genes.Endogenes[i2];
				if (sourceEndogenes[i2].Overridden)
				{
					gene2.overriddenByGene = newPawn.genes.GenesListForReading.First((Gene e) => e.def == sourceEndogenes[i2].overriddenByGene.def);
				}
				else
				{
					gene2.overriddenByGene = null;
				}
			}
		}

		private static void CopyApperance(Pawn pawn, Pawn newPawn)
		{
			newPawn.story.headType = pawn.story.headType;
			newPawn.story.bodyType = pawn.story.bodyType;
			newPawn.story.hairDef = pawn.story.hairDef;
			newPawn.story.HairColor = pawn.story.HairColor;
			newPawn.story.SkinColorBase = pawn.story.SkinColorBase;
			newPawn.story.skinColorOverride = pawn.story.skinColorOverride;
			newPawn.story.furDef = pawn.story.furDef;
		}

		private static void CopyStyle(Pawn pawn, Pawn newPawn)
		{
			newPawn.style.beardDef = pawn.style.beardDef;
			if (ModsConfig.IdeologyActive)
			{
				newPawn.style.BodyTattoo = pawn.style.BodyTattoo;
				newPawn.style.FaceTattoo = pawn.style.FaceTattoo;
			}
		}

		private static void CopySkills(Pawn pawn, Pawn newPawn)
		{
			newPawn.skills.skills.Clear();
			foreach (SkillRecord skill in pawn.skills.skills)
			{
				SkillRecord item = new SkillRecord(newPawn, skill.def)
				{
					levelInt = skill.levelInt,
					passion = skill.passion,
					xpSinceLastLevel = skill.xpSinceLastLevel,
					xpSinceMidnight = skill.xpSinceMidnight
				};
				newPawn.skills.skills.Add(item);
			}
		}

		private static void CopyHediffs(Pawn pawn, Pawn newPawn)
		{
			newPawn.health.hediffSet.hediffs.Clear();
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			foreach (Hediff item in hediffs)
			{
				if (item.def.duplicationAllowed && (item.Part == null || newPawn.health.hediffSet.HasBodyPart(item.Part)) && (!(item is Hediff_AddedPart) || item.def.organicAddedBodypart) && (!(item is Hediff_Implant) || item.def.organicAddedBodypart))
				{
					Hediff hediff = HediffMaker.MakeHediff(item.def, newPawn, item.Part);
					hediff.CopyFrom(item);
					newPawn.health.hediffSet.AddDirect(hediff);
				}
			}
			foreach (Hediff item2 in hediffs)
			{
				if (item2 is Hediff_AddedPart && !item2.def.organicAddedBodypart)
				{
					newPawn.health.RestorePart(item2.Part, null, checkStateChange: false);
				}
			}
		}

		private static void CopyNeeds(Pawn pawn, Pawn newPawn)
		{
			newPawn.needs.AllNeeds.Clear();
			foreach (Need allNeed in pawn.needs.AllNeeds)
			{
				Need need = (Need)Activator.CreateInstance(allNeed.def.needClass, newPawn);
				need.def = allNeed.def;
				newPawn.needs.AllNeeds.Add(need);
				need.SetInitialLevel();
				need.CurLevel = allNeed.CurLevel;
				newPawn.needs.BindDirectNeedFields();
			}
			if (pawn.needs.mood == null)
			{
				return;
			}
			List<Thought_Memory> memories = newPawn.needs.mood.thoughts.memories.Memories;
			memories.Clear();
			foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
			{
				Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(memory.def);
				thought_Memory.CopyFrom(memory);
				thought_Memory.pawn = newPawn;
				memories.Add(thought_Memory);
			}
		}

		public void AddDuplicate(int duplicateOf, Pawn pawn)
		{
			if (duplicates.ContainsKey(duplicateOf))
			{
				duplicates[duplicateOf].Add(pawn);
				return;
			}
			duplicates.Add(duplicateOf, new DuplicateSet());
			duplicates[duplicateOf].Add(pawn);
		}

		public void RemoveDuplicate(int duplicateOf, Pawn pawn)
		{
			if (!duplicates.ContainsKey(duplicateOf))
			{
				return;
			}
			DuplicateSet duplicateSet = duplicates[duplicateOf];
			duplicateSet.Remove(pawn);
			if (duplicateSet.Contains(null))
			{
				duplicateSet.Remove(null);
			}
			Pawn pawn2 = null;
			foreach (Pawn item in duplicateSet)
			{
				if (item.health.hediffSet.GetFirstHediff<Hediff_DuplicateSickness>() != null)
				{
					if (pawn2 != null)
					{
						pawn2 = null;
						break;
					}
					pawn2 = item;
				}
			}
			if (pawn2 != null)
			{
				Hediff_DuplicateSickness firstHediff = pawn2.health.hediffSet.GetFirstHediff<Hediff_DuplicateSickness>();
				if (firstHediff != null)
				{
					firstHediff.GetComp<HediffComp_SeverityPerDay>().severityPerDay = -0.5f;
				}
			}
			if (duplicateSet.Count == 1)
			{
				duplicateSet.First().duplicate.duplicateOf = int.MinValue;
				duplicateSet.Clear();
				duplicates.Remove(duplicateOf);
			}
			else if (duplicateSet.Count == 0)
			{
				duplicates.Remove(duplicateOf);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref duplicates, "duplicates");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && duplicates == null)
			{
				duplicates = new Dictionary<int, DuplicateSet>();
			}
		}
	}
}
