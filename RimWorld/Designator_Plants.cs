using Verse;

namespace RimWorld
{
	public abstract class Designator_Plants : Designator
	{
		protected DesignationDef designationDef;

		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => designationDef;

		public Designator_Plants()
		{
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (t.def.plant == null)
			{
				return false;
			}
			if (base.Map.designationManager.DesignationOn(t, designationDef) != null)
			{
				return false;
			}
			return true;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map) || c.Fogged(base.Map))
			{
				return false;
			}
			Plant plant = c.GetPlant(base.Map);
			if (plant == null)
			{
				return "MessageMustDesignatePlants".Translate();
			}
			AcceptanceReport result = CanDesignateThing(plant);
			if (!result.Accepted)
			{
				return result;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			DesignateThing(c.GetPlant(base.Map));
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.RemoveAllDesignationsOn(t);
			base.Map.designationManager.AddDesignation(new Designation(t, designationDef));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
