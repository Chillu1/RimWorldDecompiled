namespace Verse
{
	public struct LabelValue
	{
		private string label;

		private string value;

		public string Label => label;

		public string Value => value;

		public LabelValue(string label, string value)
		{
			this = default(LabelValue);
			this.label = label;
			this.value = value;
		}

		public override string ToString()
		{
			return label;
		}
	}
}
