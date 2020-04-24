namespace Verse
{
	public abstract class IngredientValueGetter
	{
		public abstract float ValuePerUnitOf(ThingDef t);

		public abstract string BillRequirementsDescription(RecipeDef r, IngredientCount ing);

		public virtual string ExtraDescriptionLine(RecipeDef r)
		{
			return null;
		}
	}
}
