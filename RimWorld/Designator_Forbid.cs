using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Forbid : Designator
	{
		public override int DraggableDimensions => 2;

		public Designator_Forbid()
		{
			defaultLabel = "DesignatorForbid".Translate();
			defaultDesc = "DesignatorForbidDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOn");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Claim;
			hotKey = KeyBindingDefOf.Command_ItemForbid;
			hasDesignateAllFloatMenuOption = true;
			designateAllLabel = "ForbidAllItems".Translate();
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map) || c.Fogged(base.Map))
			{
				return false;
			}
			if (!c.GetThingList(base.Map).Any((Thing t) => CanDesignateThing(t).Accepted))
			{
				return "MessageMustDesignateForbiddable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (CanDesignateThing(thingList[i]).Accepted)
				{
					DesignateThing(thingList[i]);
				}
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (t.def.category != ThingCategory.Item)
			{
				return false;
			}
			CompForbiddable compForbiddable = t.TryGetComp<CompForbiddable>();
			return compForbiddable != null && !compForbiddable.Forbidden;
		}

		public override void DesignateThing(Thing t)
		{
			t.SetForbidden(value: true, warnOnFail: false);
		}
	}
}
