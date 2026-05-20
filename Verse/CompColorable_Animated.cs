namespace Verse
{
	public class CompColorable_Animated : CompColorable
	{
		public int colorOffset;

		public CompProperties_ColorableAnimated Props => (CompProperties_ColorableAnimated)props;

		public override void Initialize(CompProperties props)
		{
			base.props = props;
			if (Props.startWithRandom)
			{
				colorOffset = Rand.RangeInclusive(0, Props.colors.Count - 1);
			}
			SetColor(Props.colors[colorOffset % Props.colors.Count]);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(Props.changeInterval))
			{
				SetColor(Props.colors[colorOffset % Props.colors.Count]);
				colorOffset++;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref colorOffset, "colorOffset", 0);
		}
	}
}
