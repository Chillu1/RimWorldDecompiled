using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI;

public class MentalBreakWorker
{
	public MentalBreakDef def;

	public virtual float CommonalityFor(Pawn pawn, bool moodCaused = false)
	{
		float num = def.baseCommonality;
		if (pawn.Faction == Faction.OfPlayer && def.commonalityFactorPerPopulationCurve != null)
		{
			num *= def.commonalityFactorPerPopulationCurve.Evaluate(PawnsFinder.AllMaps_FreeColonists.Count());
		}
		if (pawn.genes != null && def.mentalState != null && def.mentalState.IsAggro)
		{
			num *= pawn.genes.AggroMentalBreakSelectionChanceFactor;
		}
		return num;
	}

	public virtual bool BreakCanOccur(Pawn pawn)
	{
		if (def.requiredTrait != null && (pawn.story == null || !pawn.story.traits.HasTrait(def.requiredTrait)))
		{
			return false;
		}
		if (def.requiredGene != null && (pawn.genes == null || !pawn.genes.HasActiveGene(def.requiredGene)))
		{
			return false;
		}
		if (def.requiredPrecept != null && (pawn.Ideo == null || !pawn.Ideo.HasPrecept(def.requiredPrecept)))
		{
			return false;
		}
		if (!def.layerWhitelist.NullOrEmpty() && (!pawn.SpawnedOrAnyParentSpawned || !pawn.MapHeld.Tile.Valid || !def.layerWhitelist.Contains(pawn.MapHeld.Tile.LayerDef)))
		{
			return false;
		}
		if (def.mentalState != null && pawn.story != null && pawn.story.traits.allTraits.Any((Trait tr) => !tr.Suppressed && tr.CurrentData.disallowedMentalStates != null && tr.CurrentData.disallowedMentalStates.Contains(def.mentalState)))
		{
			return false;
		}
		if (!def.questLodgersCanDo && pawn.IsQuestLodger())
		{
			return false;
		}
		if (def.mentalState != null && !def.mentalState.Worker.StateCanOccur(pawn))
		{
			return false;
		}
		if (pawn.story != null)
		{
			IEnumerable<MentalBreakDef> theOnlyAllowedMentalBreaks = pawn.story.traits.TheOnlyAllowedMentalBreaks;
			if (!theOnlyAllowedMentalBreaks.Contains(def) && theOnlyAllowedMentalBreaks.Any((MentalBreakDef x) => x.intensity == def.intensity && x.Worker.BreakCanOccur(pawn)))
			{
				return false;
			}
		}
		if (TutorSystem.TutorialMode && pawn.Faction == Faction.OfPlayer)
		{
			return false;
		}
		return true;
	}

	public virtual bool TryStart(Pawn pawn, string reason, bool causedByMood)
	{
		if (pawn.mindState?.mentalStateHandler == null)
		{
			return false;
		}
		return pawn.mindState.mentalStateHandler.TryStartMentalState(def.mentalState, reason, forced: false, forceWake: false, causedByMood);
	}

	protected bool TrySendLetter(Pawn pawn, string textKey, string reason)
	{
		if (!PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			return false;
		}
		TaggedString label = def.LabelCap + ": " + pawn.LabelShortCap;
		TaggedString taggedString = textKey.Translate(pawn.LabelShort, pawn.Named("PAWN")).CapitalizeFirst();
		if (reason != null)
		{
			taggedString += "\n\n" + reason;
		}
		taggedString = taggedString.AdjustedFor(pawn);
		Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NegativeEvent, pawn);
		return true;
	}
}
