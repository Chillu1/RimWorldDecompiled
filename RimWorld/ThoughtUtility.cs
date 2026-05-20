using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public static class ThoughtUtility
	{
		public static List<ThoughtDef> situationalSocialThoughtDefs;

		public static List<ThoughtDef> situationalNonSocialThoughtDefs;

		public static void Reset()
		{
			situationalSocialThoughtDefs = DefDatabase<ThoughtDef>.AllDefs.Where((ThoughtDef x) => x.IsSituational && x.IsSocial).ToList();
			situationalNonSocialThoughtDefs = DefDatabase<ThoughtDef>.AllDefs.Where((ThoughtDef x) => x.IsSituational && !x.IsSocial).ToList();
		}

		public static void GiveThoughtsForPawnExecuted(Pawn victim, Pawn executioner, PawnExecutionKind kind)
		{
			if (!victim.RaceProps.Humanlike)
			{
				return;
			}
			int num = 0;
			if (victim.guilt.IsGuilty)
			{
				num = 0;
			}
			else
			{
				switch (kind)
				{
				case PawnExecutionKind.GenericHumane:
					num = 1;
					break;
				case PawnExecutionKind.GenericBrutal:
					num = 2;
					break;
				case PawnExecutionKind.OrganHarvesting:
					num = 3;
					break;
				case PawnExecutionKind.Ripscanned:
					num = (ModsConfig.BiotechActive ? 4 : 3);
					break;
				}
			}
			if (victim.IsPrisoner)
			{
				if (executioner?.Faction != null)
				{
					executioner.Faction.lastExecutionTick = Find.TickManager.TicksGame;
				}
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ExecutedPrisoner, executioner.Named(HistoryEventArgsNames.Doer), num.Named(HistoryEventArgsNames.ExecutionThoughtStage)));
				if (victim.guilt.IsGuilty)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ExecutedPrisonerGuilty, executioner.Named(HistoryEventArgsNames.Doer)));
				}
				else
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ExecutedPrisonerInnocent, executioner.Named(HistoryEventArgsNames.Doer)));
				}
			}
			else if (victim.HostFaction != null)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ExecutedGuest, executioner.Named(HistoryEventArgsNames.Doer), num.Named(HistoryEventArgsNames.ExecutionThoughtStage)));
			}
			else
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ExecutedColonist, executioner.Named(HistoryEventArgsNames.Doer), num.Named(HistoryEventArgsNames.ExecutionThoughtStage)));
			}
		}

		public static void GiveThoughtsForPawnOrganHarvested(Pawn victim, Pawn billDoer)
		{
			if (victim.RaceProps.Humanlike)
			{
				if (victim.needs.mood != null)
				{
					victim.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested);
				}
				if (ModsConfig.IdeologyActive)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.HarvestedOrgan, billDoer.Named(HistoryEventArgsNames.Doer)));
				}
				if (billDoer.needs.mood != null && billDoer.story?.traits != null && billDoer.story.traits.HasTrait(TraitDefOf.Bloodlust))
				{
					billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.HarvestedOrgan_Bloodlust);
				}
				if (victim.IsColonist)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.HarvestedOrganFromColonist, billDoer.Named(HistoryEventArgsNames.Doer)));
				}
				else if (victim.HostFaction == Faction.OfPlayer)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.HarvestedOrganFromGuest, billDoer.Named(HistoryEventArgsNames.Doer)));
				}
			}
		}

		public static Gene NullifyingGene(ThoughtDef def, Pawn pawn)
		{
			if (!ModsConfig.BiotechActive)
			{
				return null;
			}
			if (def.nullifyingGenes != null && pawn.genes != null)
			{
				for (int i = 0; i < def.nullifyingGenes.Count; i++)
				{
					Gene gene = pawn.genes.GetGene(def.nullifyingGenes[i]);
					if (gene != null)
					{
						return gene;
					}
				}
			}
			return null;
		}

		public static Hediff NullifyingHediff(ThoughtDef def, Pawn pawn)
		{
			return pawn.health.hediffSet.ThoughtNullifyingHediff(def);
		}

		public static bool NeverNullified(ThoughtDef def, Pawn pawn)
		{
			if (!def.neverNullifyIfAnyTrait.NullOrEmpty())
			{
				for (int i = 0; i < def.neverNullifyIfAnyTrait.Count; i++)
				{
					if (pawn.story.traits.GetTrait(def.neverNullifyIfAnyTrait[i]) != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static Trait NullifyingTrait(ThoughtDef def, Pawn pawn)
		{
			if (def.nullifyingTraits != null)
			{
				for (int i = 0; i < def.nullifyingTraits.Count; i++)
				{
					Trait trait = pawn.story.traits.GetTrait(def.nullifyingTraits[i]);
					if (trait != null)
					{
						return trait;
					}
				}
			}
			if (def.nullifyingTraitDegrees != null)
			{
				for (int j = 0; j < def.nullifyingTraitDegrees.Count; j++)
				{
					Trait trait2 = def.nullifyingTraitDegrees[j].GetTrait(pawn);
					if (trait2 != null)
					{
						return trait2;
					}
				}
			}
			return null;
		}

		public static TaleDef NullifyingTale(ThoughtDef def, Pawn pawn)
		{
			if (def.nullifyingOwnTales != null)
			{
				for (int i = 0; i < def.nullifyingOwnTales.Count; i++)
				{
					if (Find.TaleManager.GetLatestTale(def.nullifyingOwnTales[i], pawn) != null)
					{
						return def.nullifyingOwnTales[i];
					}
				}
			}
			return null;
		}

		public static PreceptDef NullifyingPrecept(ThoughtDef def, Pawn pawn)
		{
			if (def.nullifyingPrecepts != null)
			{
				for (int i = 0; i < def.nullifyingPrecepts.Count; i++)
				{
					if (pawn.Ideo != null && pawn.Ideo.HasPrecept(def.nullifyingPrecepts[i]))
					{
						return def.nullifyingPrecepts[i];
					}
				}
			}
			return null;
		}

		public static void RemovePositiveBedroomThoughts(Pawn pawn)
		{
			if (pawn?.needs?.mood != null)
			{
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBedroom, (Thought_Memory thought) => thought.MoodOffset() > 0f);
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBarracks, (Thought_Memory thought) => thought.MoodOffset() > 0f);
			}
		}

		public static bool CanGetThought(Pawn pawn, ThoughtDef def, bool checkIfNullified = false)
		{
			try
			{
				if (!def.developmentalStageFilter.Has(pawn.DevelopmentalStage))
				{
					return false;
				}
				if (def.gender != Gender.None && pawn.gender != def.gender && !def.IsSocial)
				{
					return false;
				}
				if (def.doNotApplyToQuestLodgers && pawn.IsQuestLodger())
				{
					return false;
				}
				if (def.minExpectation != null)
				{
					if (!pawn.Spawned)
					{
						return false;
					}
					ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(pawn.MapHeld);
					if (expectationDef != null && expectationDef.order < def.minExpectation.order)
					{
						return false;
					}
				}
				if (!def.validWhileDespawned && !pawn.Spawned && !def.IsMemory)
				{
					return false;
				}
				if (pawn.story.traits.IsThoughtDisallowed(def))
				{
					return false;
				}
				bool flag = false;
				bool flag2 = false;
				if (!def.requiredHediffs.NullOrEmpty())
				{
					flag = true;
					for (int i = 0; i < def.requiredHediffs.Count; i++)
					{
						if (pawn.health.hediffSet.HasHediff(def.requiredHediffs[i]))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!def.requiredTraits.NullOrEmpty() && !flag2)
				{
					flag = true;
					for (int j = 0; j < def.requiredTraits.Count; j++)
					{
						if (pawn.story.traits.HasTrait(def.requiredTraits[j]) && (!def.RequiresSpecificTraitsDegree || def.requiredTraitsDegree == pawn.story.traits.DegreeOfTrait(def.requiredTraits[j])))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag && !flag2)
				{
					return false;
				}
				if (ModsConfig.BiotechActive && !def.requiredGenes.NullOrEmpty())
				{
					if (pawn.genes == null)
					{
						return false;
					}
					for (int k = 0; k < def.requiredGenes.Count; k++)
					{
						Gene gene = pawn.genes.GetGene(def.requiredGenes[k]);
						if (gene == null || !gene.Active)
						{
							return false;
						}
					}
				}
				if (def.nullifiedIfNotColonist && !pawn.IsColonist)
				{
					return false;
				}
				if (checkIfNullified && ThoughtNullified(pawn, def))
				{
					return false;
				}
			}
			finally
			{
			}
			return true;
		}

		public static bool ThoughtNullified(Pawn pawn, ThoughtDef def)
		{
			if (NeverNullified(def, pawn))
			{
				return false;
			}
			if (NullifyingTrait(def, pawn) != null)
			{
				return true;
			}
			if (NullifyingHediff(def, pawn) != null)
			{
				return true;
			}
			if (NullifyingTale(def, pawn) != null)
			{
				return true;
			}
			if (NullifyingPrecept(def, pawn) != null)
			{
				return true;
			}
			if (NullifyingGene(def, pawn) != null)
			{
				return true;
			}
			return false;
		}

		public static string ThoughtNullifiedMessage(Pawn pawn, ThoughtDef def)
		{
			if (NeverNullified(def, pawn))
			{
				return "";
			}
			Trait trait = NullifyingTrait(def, pawn);
			if (trait != null)
			{
				return "ThoughtNullifiedBy".Translate().CapitalizeFirst() + ": " + trait.LabelCap;
			}
			Hediff hediff = NullifyingHediff(def, pawn);
			if (hediff != null)
			{
				return "ThoughtNullifiedBy".Translate().CapitalizeFirst() + ": " + hediff.def.LabelCap;
			}
			TaleDef taleDef = NullifyingTale(def, pawn);
			if (taleDef != null)
			{
				return "ThoughtNullifiedBy".Translate().CapitalizeFirst() + ": " + taleDef.LabelCap;
			}
			PreceptDef preceptDef = NullifyingPrecept(def, pawn);
			if (preceptDef != null)
			{
				return "DisabledByPrecept".Translate(preceptDef.issue.LabelCap) + ": " + preceptDef.LabelCap;
			}
			Gene gene = NullifyingGene(def, pawn);
			if (gene != null)
			{
				return "DisabledByGene".Translate() + ": " + gene.LabelCap;
			}
			return "";
		}

		public static bool Witnessed(Pawn p, Pawn victim)
		{
			if (!p.Awake() || PawnUtility.IsBiologicallyOrArtificiallyBlind(p))
			{
				return false;
			}
			if (victim.IsCaravanMember())
			{
				return victim.GetCaravan() == p.GetCaravan();
			}
			if (!victim.Spawned || !p.Spawned)
			{
				return false;
			}
			if (!p.Position.InHorDistOf(victim.Position, 12f))
			{
				return false;
			}
			if (!GenSight.LineOfSight(victim.Position, p.Position, victim.Map))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<TraitRequirement> GetNullifyingTraits(ThoughtDef thoughtDef)
		{
			if (thoughtDef.nullifyingTraits != null)
			{
				for (int i = 0; i < thoughtDef.nullifyingTraits.Count; i++)
				{
					yield return new TraitRequirement
					{
						def = thoughtDef.nullifyingTraits[i]
					};
				}
			}
			if (thoughtDef.nullifyingTraitDegrees != null)
			{
				for (int i = 0; i < thoughtDef.nullifyingTraitDegrees.Count; i++)
				{
					yield return thoughtDef.nullifyingTraitDegrees[i];
				}
			}
		}

		public static string GenerateBabyTalk(string str)
		{
			string[] syllables = "BabyTalk".Translate().ToString().Split(',');
			return GenText.CapitalizeSentences(Regex.Replace(str, "(\\p{L}|')+", (Match word) => string.Join("", Enumerable.Repeat(syllables[Rand.RangeSeeded(0, syllables.Length, GenText.StableStringHash(word.ToString()))], (word.Length <= 6) ? 1 : 2))));
		}
	}
}
