using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Designator_AreaAllowed : Designator_Area
	{
		private static Area selectedArea;

		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public static Area SelectedArea => selectedArea;

		public Designator_AreaAllowed(DesignateMode mode)
		{
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
		}

		public static void ClearSelectedArea()
		{
			selectedArea = null;
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			if (selectedArea != null && Find.WindowStack.FloatMenu == null)
			{
				selectedArea.MarkForDraw();
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (CheckCanInteract())
			{
				if (selectedArea != null)
				{
					base.ProcessInput(ev);
				}
				AreaUtility.MakeAllowedAreaListFloatMenu(delegate(Area a)
				{
					selectedArea = a;
					base.ProcessInput(ev);
				}, addNullAreaOption: false, addManageOption: true, base.Map);
			}
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AllowedAreas, KnowledgeAmount.SpecificInteraction);
		}
	}
}
