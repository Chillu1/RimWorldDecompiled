namespace RimWorld
{
	public abstract class ScenPart_Rule : ScenPart
	{
		public override void PostGameStart()
		{
			ApplyRule();
		}

		protected abstract void ApplyRule();
	}
}
