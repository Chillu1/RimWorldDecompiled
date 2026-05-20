using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Slaughter : Designator
{
	private readonly List<Pawn> justDesignated = new List<Pawn>();

	protected override DesignationDef Designation => DesignationDefOf.Slaughter;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Slaughter()
	{
		defaultLabel = "DesignatorSlaughter".Translate();
		defaultDesc = "DesignatorSlaughterDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Slaughter");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Slaughter;
		hotKey = KeyBindingDefOf.Misc7;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (!SlaughterablesInCell(c).Any())
		{
			return "MessageMustDesignateSlaughterable".Translate();
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		foreach (Pawn item in SlaughterablesInCell(loc))
		{
			DesignateThing(item);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t is Pawn { IsAnimal: not false } pawn && pawn.Faction == Faction.OfPlayer && base.Map.designationManager.DesignationOn(pawn, Designation) == null && !pawn.InAggroMentalState)
		{
			return true;
		}
		return false;
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		justDesignated.Add((Pawn)t);
		Designation designation = base.Map.designationManager.DesignationOn(t, DesignationDefOf.ReleaseAnimalToWild);
		if (designation != null)
		{
			base.Map.designationManager.RemoveDesignation(designation);
		}
	}

	protected override void FinalizeDesignationSucceeded()
	{
		base.FinalizeDesignationSucceeded();
		for (int i = 0; i < justDesignated.Count; i++)
		{
			ShowDesignationWarnings(justDesignated[i]);
		}
		justDesignated.Clear();
	}

	private IEnumerable<Pawn> SlaughterablesInCell(IntVec3 c)
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

	private void ShowDesignationWarnings(Pawn pawn)
	{
		SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(pawn);
		SlaughterDesignatorUtility.CheckWarnAboutVeneratedAnimal(pawn);
	}
}
