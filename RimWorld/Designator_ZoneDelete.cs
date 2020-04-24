using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneDelete : Designator_Zone
	{
		private List<Zone> justDesignated = new List<Zone>();

		public Designator_ZoneDelete()
		{
			defaultLabel = "DesignatorZoneDelete".Translate();
			defaultDesc = "DesignatorZoneDeleteDesc".Translate();
			soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneDelete;
			useMouseIcon = true;
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneDelete");
			hotKey = KeyBindingDefOf.Misc3;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 sq)
		{
			if (!sq.InBounds(base.Map))
			{
				return false;
			}
			if (sq.Fogged(base.Map))
			{
				return false;
			}
			if (base.Map.zoneManager.ZoneAt(sq) == null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Zone zone = base.Map.zoneManager.ZoneAt(c);
			zone.RemoveCell(c);
			if (!justDesignated.Contains(zone))
			{
				justDesignated.Add(zone);
			}
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			for (int i = 0; i < justDesignated.Count; i++)
			{
				justDesignated[i].CheckContiguous();
			}
			justDesignated.Clear();
		}
	}
}
