using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Hunt : Designator
	{
		private List<Pawn> justDesignated = new List<Pawn>();

		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Hunt;

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
			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.AnimalOrWildMan() && !pawn.IsPrisonerInPrisonCell() && (pawn.Faction == null || !pawn.Faction.def.humanlikeFaction) && base.Map.designationManager.DesignationOn(pawn, Designation) == null)
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

		private void ShowDesignationWarnings(Pawn pawn)
		{
			float manhunterOnDamageChance = pawn.RaceProps.manhunterOnDamageChance;
			float manhunterOnDamageChance2 = PawnUtility.GetManhunterOnDamageChance(pawn.kindDef);
			if (manhunterOnDamageChance >= 0.015f)
			{
				Messages.Message("MessageAnimalsGoPsychoHunted".Translate(pawn.kindDef.GetLabelPlural().CapitalizeFirst(), manhunterOnDamageChance2.ToStringPercent(), pawn.Named("ANIMAL")).CapitalizeFirst(), pawn, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
	}
}
