namespace Verse
{
	public class BodyPartGroupDef : Def
	{
		[MustTranslate]
		public string labelShort;

		public int listOrder;

		[Unsaved(false)]
		private string cachedLabelShortCap;

		public string LabelShort
		{
			get
			{
				if (!labelShort.NullOrEmpty())
				{
					return labelShort;
				}
				return label;
			}
		}

		public string LabelShortCap
		{
			get
			{
				if (labelShort.NullOrEmpty())
				{
					return base.LabelCap;
				}
				if (cachedLabelShortCap == null)
				{
					cachedLabelShortCap = labelShort.CapitalizeFirst();
				}
				return cachedLabelShortCap;
			}
		}
	}
}
