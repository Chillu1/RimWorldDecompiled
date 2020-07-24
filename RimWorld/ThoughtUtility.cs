using System;
using System.Collections.Generic;
using System.Linq;
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

		public static void GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
		{
			if (!victim.RaceProps.Humanlike)
			{
				return;
			}
			int forcedStage = 1;
			if (victim.guilt.IsGuilty)
			{
				forcedStage = 0;
			}
			else
			{
				switch (kind)
				{
				case PawnExecutionKind.GenericHumane:
					forcedStage = 1;
					break;
				case PawnExecutionKind.GenericBrutal:
					forcedStage = 2;
					break;
				case PawnExecutionKind.OrganHarvesting:
					forcedStage = 3;
					break;
				}
			}
			ThoughtDef def = (!victim.IsColonist) ? ThoughtDefOf.KnowGuestExecuted : ThoughtDefOf.KnowColonistExecuted;
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
			{
				if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.IsColonist && allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood != null)
				{
					allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, forcedStage));
				}
			}
		}

		public static void GiveThoughtsForPawnOrganHarvested(Pawn victim)
		{
			if (!victim.RaceProps.Humanlike)
			{
				return;
			}
			ThoughtDef thoughtDef = null;
			if (victim.IsColonist)
			{
				thoughtDef = ThoughtDefOf.KnowColonistOrganHarvested;
			}
			else if (victim.HostFaction == Faction.OfPlayer)
			{
				thoughtDef = ThoughtDefOf.KnowGuestOrganHarvested;
			}
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
			{
				if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood != null)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner == victim)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested);
					}
					else if (thoughtDef != null)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
					}
				}
			}
		}

		public static Hediff NullifyingHediff(ThoughtDef def, Pawn pawn)
		{
			if (def.IsMemory)
			{
				return null;
			}
			float num = 0f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			Hediff result = null;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.pctConditionalThoughtsNullified > num)
				{
					num = curStage.pctConditionalThoughtsNullified;
					result = hediffs[i];
				}
			}
			if (num == 0f)
			{
				return null;
			}
			Rand.PushState();
			Rand.Seed = pawn.thingIDNumber * 31 + def.index * 139;
			bool num2 = Rand.Value < num;
			Rand.PopState();
			if (!num2)
			{
				return null;
			}
			return result;
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

		public static void RemovePositiveBedroomThoughts(Pawn pawn)
		{
			if (pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBedroom, (Thought_Memory thought) => thought.MoodOffset() > 0f);
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBarracks, (Thought_Memory thought) => thought.MoodOffset() > 0f);
			}
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		public static bool CanGetThought(Pawn pawn, ThoughtDef def)
		{
			return CanGetThought_NewTemp(pawn, def);
		}

		public static bool CanGetThought_NewTemp(Pawn pawn, ThoughtDef def, bool checkIfNullified = false)
		{
			try
			{
				if (!def.validWhileDespawned && !pawn.Spawned && !def.IsMemory)
				{
					return false;
				}
				if (!def.requiredTraits.NullOrEmpty())
				{
					bool flag = false;
					for (int i = 0; i < def.requiredTraits.Count; i++)
					{
						if (pawn.story.traits.HasTrait(def.requiredTraits[i]) && (!def.RequiresSpecificTraitsDegree || def.requiredTraitsDegree == pawn.story.traits.DegreeOfTrait(def.requiredTraits[i])))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
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
			return false;
		}

		public static string ThoughtNullifiedMessage(Pawn pawn, ThoughtDef def)
		{
			TaggedString t = "ThoughtNullifiedBy".Translate().CapitalizeFirst() + ": ";
			Trait trait = NullifyingTrait(def, pawn);
			if (trait != null)
			{
				return t + trait.LabelCap;
			}
			Hediff hediff = NullifyingHediff(def, pawn);
			if (hediff != null)
			{
				return t + hediff.def.LabelCap;
			}
			TaleDef taleDef = NullifyingTale(def, pawn);
			if (taleDef != null)
			{
				return t + taleDef.LabelCap;
			}
			return "";
		}
	}
}
