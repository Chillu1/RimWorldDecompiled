using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_DropEquipment : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool CanSelfTarget => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return context.FirstSelectedPawn.equipment?.Primary != null;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (clickedPawn != context.FirstSelectedPawn)
		{
			return null;
		}
		if (clickedPawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanUnequip(clickedPawn.equipment.Primary, clickedPawn))
		{
			return new FloatMenuOption("CannotDrop".Translate(clickedPawn.equipment.Primary.Label, clickedPawn.equipment.Primary) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null);
		}
		Action action = delegate
		{
			clickedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, clickedPawn.equipment.Primary), JobTag.Misc);
		};
		return new FloatMenuOption("Drop".Translate(clickedPawn.equipment.Primary.Label, clickedPawn.equipment.Primary), action, clickedPawn.equipment.Primary, Color.white, MenuOptionPriority.Default, null, clickedPawn);
	}
}
