using Verse;

namespace RimWorld
{
	public abstract class Designator_AreaSnowClear : Designator_Area
	{
		private DesignateMode mode;

		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_AreaSnowClear(DesignateMode mode)
		{
			this.mode = mode;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			hotKey = KeyBindingDefOf.Misc7;
			tutorTag = "AreaSnowClear";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			bool flag = base.Map.areaManager.SnowClear[c];
			if (mode == DesignateMode.Add)
			{
				return !flag;
			}
			return flag;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (mode == DesignateMode.Add)
			{
				base.Map.areaManager.SnowClear[c] = true;
			}
			else
			{
				base.Map.areaManager.SnowClear[c] = false;
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.SnowClear.MarkForDraw();
		}
	}
}
