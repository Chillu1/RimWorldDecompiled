using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Open : Designator
	{
		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Open;

		public Designator_Open()
		{
			defaultLabel = "DesignatorOpen".Translate();
			defaultDesc = "DesignatorOpenDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Open");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			hotKey = KeyBindingDefOf.Misc5;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Claim;
		}

		protected override void FinalizeDesignationFailed()
		{
			base.FinalizeDesignationFailed();
			Messages.Message("MessageMustDesignateOpenable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!OpenablesInCell(c).Any())
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Thing item in OpenablesInCell(c))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			IOpenable openable = t as IOpenable;
			if (openable == null || !openable.CanOpen || base.Map.designationManager.DesignationOn(t, Designation) != null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		}

		private IEnumerable<Thing> OpenablesInCell(IntVec3 c)
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
					yield return thingList[i];
				}
			}
		}
	}
}
