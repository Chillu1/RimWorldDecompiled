using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_RemoveBodyPart : Recipe_Surgery
{
	protected virtual bool SpawnPartsWhenRemoved => true;

	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts();
		foreach (BodyPartRecord part in notMissingParts)
		{
			if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
			{
				yield return part;
			}
			else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
			{
				yield return part;
			}
			else if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => (!(d is Hediff_Injury) || d.IsPermanent()) && d.def.isBad && d.Visible && d.Part == part))
			{
				yield return part;
			}
			else if (part.def.forceAlwaysRemovable)
			{
				yield return part;
			}
		}
	}

	public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
	{
		if ((pawn.Faction == billDoerFaction || pawn.Faction == null) && !pawn.IsQuestLodger())
		{
			return false;
		}
		if (HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest)
		{
			return true;
		}
		return false;
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		bool flag = MedicalRecipesUtility.IsClean(pawn, part);
		bool flag2 = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
		if (billDoer != null)
		{
			if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
			{
				return;
			}
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			pawn.health.hediffSet.GetDirectlyAddedPartFor(part)?.Notify_SurgicallyRemoved(billDoer);
			if (SpawnPartsWhenRemoved)
			{
				MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
				MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
			}
		}
		DamagePart(pawn, part);
		pawn.Drawer.renderer.SetAllGraphicsDirty();
		if (flag)
		{
			ApplyThoughts(pawn, billDoer);
		}
		if (flag2)
		{
			ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
		}
	}

	public virtual void DamagePart(Pawn pawn, BodyPartRecord part)
	{
		pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, part));
	}

	public virtual void ApplyThoughts(Pawn pawn, Pawn billDoer)
	{
		if (pawn.Dead)
		{
			ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.OrganHarvesting);
		}
		else
		{
			ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
		}
	}

	public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
	{
		if (!part.def.removeRecipeLabelOverride.NullOrEmpty())
		{
			return part.def.removeRecipeLabelOverride;
		}
		if (pawn.RaceProps.IsMechanoid || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
		{
			if (pawn.health.hediffSet.TryGetDirectlyAddedPartFor(part, out var hediff))
			{
				return "RemovePart".Translate(hediff.Label);
			}
			return RecipeDefOf.RemoveBodyPart.label;
		}
		switch (HealthUtility.PartRemovalIntent(pawn, part))
		{
		case BodyPartRemovalIntent.Amputate:
			if (part.depth == BodyPartDepth.Inside || part.def.socketed)
			{
				return "RemoveOrgan".Translate();
			}
			return "Amputate".Translate();
		case BodyPartRemovalIntent.Harvest:
			return "HarvestOrgan".Translate();
		default:
			throw new InvalidOperationException();
		}
	}
}
