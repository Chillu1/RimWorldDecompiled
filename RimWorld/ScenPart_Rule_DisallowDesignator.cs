using Verse;

namespace RimWorld
{
	public class ScenPart_Rule_DisallowDesignator : ScenPart_Rule
	{
		protected override void ApplyRule()
		{
			Current.Game.Rules.SetAllowDesignator(def.designatorType, allowed: false);
		}
	}
}
