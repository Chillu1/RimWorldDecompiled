using Verse;

namespace RimWorld
{
	public class StyleItemTagWeighted
	{
		[NoTranslate]
		private string tag;

		private float baseWeight;

		private float weightFactor;

		public string Tag => tag;

		public float TotalWeight => baseWeight * weightFactor;

		public StyleItemTagWeighted()
		{
		}

		public StyleItemTagWeighted(string tag, float baseWeight, float weightFactor = 1f)
		{
			this.tag = tag;
			this.baseWeight = baseWeight;
			this.weightFactor = weightFactor;
		}

		public void Add(StyleItemTagWeighted other)
		{
			baseWeight += other.baseWeight;
			weightFactor *= other.weightFactor;
		}
	}
}
