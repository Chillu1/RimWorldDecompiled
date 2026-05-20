using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_ReleaseAnimalToWild : Designator
{
	public Designator_ReleaseAnimalToWild()
	{
		defaultLabel = "DesignatorReleaseAnimalToWild".Translate();
		defaultDesc = "DesignatorReleaseAnimalToWildDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/ReleaseToTheWild");
		useMouseIcon = true;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		soundSucceeded = SoundDefOf.Designate_ReleaseToWild;
		hotKey = KeyBindingDefOf.Misc3;
		tutorTag = "ReleaseAnimalToWild";
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		foreach (Thing thing in c.GetThingList(base.Map))
		{
			if (CanDesignateThing(thing).Accepted)
			{
				return true;
			}
		}
		return false;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		return t is Pawn { IsAnimal: not false } pawn && pawn.Faction == Faction.OfPlayer && base.Map.designationManager.DesignationOn(t, DesignationDefOf.ReleaseAnimalToWild) == null && !pawn.Dead && pawn.RaceProps.canReleaseToWild;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		foreach (Thing thing in c.GetThingList(base.Map))
		{
			if (CanDesignateThing(thing).Accepted)
			{
				DesignateThing(thing);
			}
		}
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.AddDesignation(new Designation((Pawn)t, DesignationDefOf.ReleaseAnimalToWild));
		Designation designation = base.Map.designationManager.DesignationOn(t, DesignationDefOf.Slaughter);
		if (designation != null)
		{
			base.Map.designationManager.RemoveDesignation(designation);
		}
		ReleaseAnimalToWildUtility.CheckWarnAboutBondedAnimal((Pawn)t);
	}
}
