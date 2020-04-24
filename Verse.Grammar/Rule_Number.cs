namespace Verse.Grammar
{
	public class Rule_Number : Rule
	{
		private IntRange range = IntRange.zero;

		public int selectionWeight = 1;

		public override float BaseSelectionWeight => selectionWeight;

		public override Rule DeepCopy()
		{
			Rule_Number obj = (Rule_Number)base.DeepCopy();
			obj.range = range;
			obj.selectionWeight = selectionWeight;
			return obj;
		}

		public override string Generate()
		{
			return range.RandomInRange.ToString();
		}

		public override string ToString()
		{
			return keyword + "->(number: " + range.ToString() + ")";
		}
	}
}
