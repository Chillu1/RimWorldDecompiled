using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Tame : Designator
{
	private readonly List<Pawn> justDesignated = new List<Pawn>();

	protected override DesignationDef Designation => DesignationDefOf.Tame;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Tame()
	{
		defaultLabel = "DesignatorTame".Translate();
		defaultDesc = "DesignatorTameDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Tame");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Tame;
		hotKey = KeyBindingDefOf.Misc4;
		tutorTag = "Tame";
		showReverseDesignatorDisabledReason = true;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (!TameablesInCell(c).Any())
		{
			return "MessageMustDesignateTameable".Translate();
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		foreach (Pawn item in TameablesInCell(loc))
		{
			DesignateThing(item);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!(t is Pawn pawn))
		{
			return false;
		}
		if (!TameUtility.CanTame(pawn))
		{
			return false;
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria))
		{
			return "CantTameScaria".Translate();
		}
		return base.Map.designationManager.DesignationOn(pawn, Designation) == null;
	}

	protected override void FinalizeDesignationSucceeded()
	{
		base.FinalizeDesignationSucceeded();
		foreach (PawnKindDef kind in justDesignated.Select((Pawn p) => p.kindDef).Distinct())
		{
			TameUtility.ShowDesignationWarnings(justDesignated.First((Pawn x) => x.kindDef == kind));
		}
		justDesignated.Clear();
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTaming, KnowledgeAmount.Total);
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.RemoveAllDesignationsOn(t);
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		justDesignated.Add((Pawn)t);
	}

	private IEnumerable<Pawn> TameablesInCell(IntVec3 c)
	{
		if (c.Fogged(base.Map))
		{
			yield break;
		}
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (CanDesignateThing(thingList[i]).Accepted)
			{
				yield return (Pawn)thingList[i];
			}
		}
	}
}
