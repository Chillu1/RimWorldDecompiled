using System;
using Verse;

namespace RimWorld;

public class PawnRelationWorker
{
	public PawnRelationDef def;

	public virtual bool InRelation(Pawn me, Pawn other)
	{
		if (def.implied)
		{
			throw new NotImplementedException(def?.ToString() + " lacks InRelation implementation.");
		}
		return me.relations.DirectRelationExists(def, other);
	}

	public virtual float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
	{
		return 0f;
	}

	public virtual void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
	{
		if (!def.implied)
		{
			generated.relations.AddDirectRelation(def, other);
			return;
		}
		throw new NotImplementedException(def?.ToString() + " lacks CreateRelation implementation.");
	}

	public float BaseGenerationChanceFactor(Pawn generated, Pawn other, PawnGenerationRequest request)
	{
		float num = 1f;
		if (other.IsDuplicate)
		{
			return 0f;
		}
		if (generated.Faction != other.Faction)
		{
			num *= 0.65f;
		}
		if (generated.HostileTo(other))
		{
			num *= 0.7f;
		}
		if (other.Faction != null && other.Faction.IsPlayer && (generated.Faction == null || !generated.Faction.IsPlayer))
		{
			num *= 0.5f;
		}
		if (other.Faction != null && other.Faction.IsPlayer)
		{
			num *= request.ColonistRelationChanceFactor;
		}
		if (other == request.ExtraPawnForExtraRelationChance)
		{
			num *= request.RelationWithExtraPawnChanceFactor;
		}
		TechLevel techLevel = ((generated.Faction != null) ? generated.Faction.def.techLevel : TechLevel.Undefined);
		TechLevel techLevel2 = ((other.Faction != null) ? other.Faction.def.techLevel : TechLevel.Undefined);
		if (techLevel != TechLevel.Undefined && techLevel2 != TechLevel.Undefined && techLevel != techLevel2)
		{
			num *= 0.85f;
		}
		if ((techLevel.IsNeolithicOrWorse() && !techLevel2.IsNeolithicOrWorse()) || (!techLevel.IsNeolithicOrWorse() && techLevel2.IsNeolithicOrWorse()))
		{
			num *= 0.03f;
		}
		return num;
	}

	public virtual void OnRelationCreated(Pawn firstPawn, Pawn secondPawn)
	{
	}

	public virtual void OnRelationRemoved(Pawn firstPawn, Pawn secondPawn)
	{
	}

	public virtual void Notify_PostRemovedByDeath(Pawn firstPawn, Pawn secondPawn)
	{
	}

	public virtual void Notify_PostRemovedLeftBehind(Pawn firstPawn, Pawn secondPawn)
	{
	}
}
