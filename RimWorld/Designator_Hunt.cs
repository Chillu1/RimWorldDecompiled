using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Hunt : Designator
{
	private readonly List<Pawn> justDesignated = new List<Pawn>();

	protected override DesignationDef Designation => DesignationDefOf.Hunt;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Hunt()
	{
		defaultLabel = "DesignatorHunt".Translate();
		defaultDesc = "DesignatorHuntDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Hunt");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Hunt;
		hotKey = KeyBindingDefOf.Misc11;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (!HuntablesInCell(c).Any())
		{
			return "MessageMustDesignateHuntable".Translate();
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		foreach (Pawn item in HuntablesInCell(loc))
		{
			DesignateThing(item);
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t is Pawn pawn && pawn.AnimalOrWildMan() && !pawn.IsPrisonerInPrisonCell() && (pawn.Faction == null || !pawn.Faction.def.humanlikeFaction) && base.Map.designationManager.DesignationOn(pawn, Designation) == null)
		{
			return true;
		}
		return false;
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.RemoveAllDesignationsOn(t);
		base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		justDesignated.Add((Pawn)t);
	}

	protected override void FinalizeDesignationSucceeded()
	{
		base.FinalizeDesignationSucceeded();
		foreach (PawnKindDef kind in justDesignated.Select((Pawn p) => p.kindDef).Distinct())
		{
			ShowDesignationWarnings(justDesignated.First((Pawn x) => x.kindDef == kind));
		}
		justDesignated.Clear();
	}

	private IEnumerable<Pawn> HuntablesInCell(IntVec3 c)
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

	public static void ShowDesignationWarnings(Pawn pawn)
	{
		CheckHunters(pawn.MapHeld, out var anyAssignedHunting, out var anyProperWeapon);
		if (!anyAssignedHunting)
		{
			Messages.Message("MessageNoHuntersAvailable".Translate(), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
		else if (!anyProperWeapon)
		{
			Messages.Message("MessageNoHuntersWithProperWeapon".Translate(), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
		float manhunterOnDamageChance = pawn.RaceProps.manhunterOnDamageChance;
		float manhunterOnDamageChance2 = PawnUtility.GetManhunterOnDamageChance(pawn);
		if (manhunterOnDamageChance >= 0.015f)
		{
			Messages.Message("MessageAnimalsGoPsychoHunted".Translate(pawn.kindDef.GetLabelPlural().CapitalizeFirst(), manhunterOnDamageChance2.ToStringPercent(), pawn.Named("ANIMAL")).CapitalizeFirst(), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
		SlaughterDesignatorUtility.CheckWarnAboutVeneratedAnimal(pawn);
	}

	private static void CheckHunters(Map map, out bool anyAssignedHunting, out bool anyProperWeapon)
	{
		anyAssignedHunting = false;
		anyProperWeapon = false;
		foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
		{
			bool flag = item.workSettings.WorkIsActive(WorkTypeDefOf.Hunting);
			if (flag)
			{
				anyAssignedHunting = true;
			}
			if (!item.Downed && flag && WorkGiver_HunterHunt.HasHuntingWeapon(item))
			{
				anyProperWeapon = true;
				break;
			}
		}
	}
}
