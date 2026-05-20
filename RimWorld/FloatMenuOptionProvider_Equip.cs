using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Equip : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return context.FirstSelectedPawn.equipment != null;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!clickedThing.HasComp<CompEquippable>())
		{
			return null;
		}
		string labelShort = clickedThing.LabelShort;
		if (clickedThing.def.IsWeapon && context.FirstSelectedPawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfViolenceLower".Translate(context.FirstSelectedPawn.LabelShort, context.FirstSelectedPawn), null);
		}
		if (clickedThing.def.IsRangedWeapon && context.FirstSelectedPawn.WorkTagIsDisabled(WorkTags.Shooting))
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfShootingLower".Translate(context.FirstSelectedPawn), null);
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (!context.FirstSelectedPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "Incapable".Translate().CapitalizeFirst(), null);
		}
		if (clickedThing.IsBurning())
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "BurningLower".Translate(), null);
		}
		if (context.FirstSelectedPawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(clickedThing, context.FirstSelectedPawn))
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null);
		}
		if (!EquipmentUtility.CanEquip(clickedThing, context.FirstSelectedPawn, out var cantReason, checkBonded: false))
		{
			return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + cantReason.CapitalizeFirst(), null);
		}
		string text = "Equip".Translate(labelShort);
		if (clickedThing.def.IsRangedWeapon && context.FirstSelectedPawn.story != null && context.FirstSelectedPawn.story.traits.HasTrait(TraitDefOf.Brawler))
		{
			text += " " + "EquipWarningBrawler".Translate();
		}
		if (EquipmentUtility.AlreadyBondedToWeapon(clickedThing, context.FirstSelectedPawn))
		{
			text += " " + "BladelinkAlreadyBonded".Translate();
			TaggedString dialogText = "BladelinkAlreadyBondedDialog".Translate(context.FirstSelectedPawn.Named("PAWN"), clickedThing.Named("WEAPON"), context.FirstSelectedPawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
			{
				Find.WindowStack.Add(new Dialog_MessageBox(dialogText));
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
		{
			string personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(clickedThing, context.FirstSelectedPawn);
			if (!personaWeaponConfirmationText.NullOrEmpty())
			{
				Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), delegate
				{
					Equip();
				}, "No".Translate()));
			}
			else
			{
				Equip();
			}
		}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
		void Equip()
		{
			clickedThing.SetForbidden(value: false);
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, clickedThing), JobTag.Misc);
			FleckMaker.Static(clickedThing.DrawPos, clickedThing.MapHeld, FleckDefOf.FeedbackEquip);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
		}
	}
}
