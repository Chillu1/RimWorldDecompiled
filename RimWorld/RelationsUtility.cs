using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class RelationsUtility
	{
		public static bool PawnsKnowEachOther(Pawn p1, Pawn p2)
		{
			if (p1.Faction != null && p1.Faction == p2.Faction)
			{
				return true;
			}
			if (p1.RaceProps.IsFlesh && p1.relations.DirectRelations.Find((DirectPawnRelation x) => x.otherPawn == p2) != null)
			{
				return true;
			}
			if (p2.RaceProps.IsFlesh && p2.relations.DirectRelations.Find((DirectPawnRelation x) => x.otherPawn == p1) != null)
			{
				return true;
			}
			if (HasAnySocialMemoryWith(p1, p2))
			{
				return true;
			}
			if (HasAnySocialMemoryWith(p2, p1))
			{
				return true;
			}
			return false;
		}

		public static bool IsDisfigured(Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Part != null && hediffs[i].Part.def.beautyRelated && (hediffs[i] is Hediff_MissingPart || hediffs[i] is Hediff_Injury))
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryDevelopBondRelation(Pawn humanlike, Pawn animal, float baseChance)
		{
			if (!animal.RaceProps.Animal)
			{
				return false;
			}
			if (animal.Faction == Faction.OfPlayer && humanlike.IsQuestLodger())
			{
				return false;
			}
			if (animal.RaceProps.trainability.intelligenceOrder < TrainabilityDefOf.Intermediate.intelligenceOrder)
			{
				return false;
			}
			if (humanlike.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
			{
				return false;
			}
			if (animal.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => x.Spawned) != null)
			{
				return false;
			}
			if (humanlike.story.traits.HasTrait(TraitDefOf.Psychopath))
			{
				return false;
			}
			int num = 0;
			List<DirectPawnRelation> directRelations = animal.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].def == PawnRelationDefOf.Bond && !directRelations[i].otherPawn.Dead)
				{
					num++;
				}
			}
			int num2 = 0;
			List<DirectPawnRelation> directRelations2 = humanlike.relations.DirectRelations;
			for (int j = 0; j < directRelations2.Count; j++)
			{
				if (directRelations2[j].def == PawnRelationDefOf.Bond && !directRelations2[j].otherPawn.Dead)
				{
					num2++;
				}
			}
			if (num > 0)
			{
				baseChance *= Mathf.Pow(0.2f, num);
			}
			if (num2 > 0)
			{
				baseChance *= Mathf.Pow(0.55f, num2);
			}
			if (Rand.Value < baseChance)
			{
				humanlike.relations.AddDirectRelation(PawnRelationDefOf.Bond, animal);
				if (humanlike.Faction == Faction.OfPlayer || animal.Faction == Faction.OfPlayer)
				{
					TaleRecorder.RecordTale(TaleDefOf.BondedWithAnimal, humanlike, animal);
				}
				bool flag = false;
				string value = null;
				if (animal.Name == null || animal.Name.Numerical)
				{
					flag = true;
					value = ((animal.Name == null) ? animal.LabelIndefinite() : animal.Name.ToStringFull);
					animal.Name = PawnBioAndNameGenerator.GeneratePawnName(animal);
				}
				if (PawnUtility.ShouldSendNotificationAbout(humanlike) || PawnUtility.ShouldSendNotificationAbout(animal))
				{
					string text = ((!flag) ? ((string)"MessageNewBondRelation".Translate(humanlike.LabelShort, animal.LabelShort, humanlike.Named("HUMAN"), animal.Named("ANIMAL")).CapitalizeFirst()) : ((string)"MessageNewBondRelationNewName".Translate(humanlike.LabelShort, value, animal.Name.ToStringFull, humanlike.Named("HUMAN"), animal.Named("ANIMAL")).AdjustedFor(animal).CapitalizeFirst()));
					Messages.Message(text, humanlike, MessageTypeDefOf.PositiveEvent);
				}
				return true;
			}
			return false;
		}

		public static string LabelWithBondInfo(Pawn humanlike, Pawn animal)
		{
			string text = humanlike.LabelShort;
			if (humanlike.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
			{
				text += " " + "BondBrackets".Translate();
			}
			return text;
		}

		private static bool HasAnySocialMemoryWith(Pawn p, Pawn otherPawn)
		{
			if (p.Dead)
			{
				return false;
			}
			if (!p.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || p.needs == null || p.needs.mood == null)
			{
				return false;
			}
			List<Thought_Memory> memories = p.needs.mood.thoughts.memories.Memories;
			for (int i = 0; i < memories.Count; i++)
			{
				Thought_MemorySocial thought_MemorySocial = memories[i] as Thought_MemorySocial;
				if (thought_MemorySocial != null && thought_MemorySocial.OtherPawn() == otherPawn)
				{
					return true;
				}
			}
			return false;
		}
	}
}
