using Verse;

namespace RimWorld
{
	public abstract class Designator_AreaHome : Designator_Area
	{
		private DesignateMode mode;

		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_AreaHome(DesignateMode mode)
		{
			this.mode = mode;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			hotKey = KeyBindingDefOf.Misc7;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			bool flag = base.Map.areaManager.Home[c];
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
				base.Map.areaManager.Home[c] = true;
			}
			else
			{
				base.Map.areaManager.Home[c] = false;
			}
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HomeArea, KnowledgeAmount.Total);
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.Home.MarkForDraw();
		}
	}
}
