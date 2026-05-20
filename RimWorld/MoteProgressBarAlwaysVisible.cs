namespace RimWorld
{
	public class MoteProgressBarAlwaysVisible : MoteProgressBar
	{
		protected override bool OnlyShowForClosestZoom => false;
	}
}
