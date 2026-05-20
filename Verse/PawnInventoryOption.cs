using System.Collections.Generic;

namespace Verse;

public class PawnInventoryOption
{
	public ThingDef thingDef;

	public IntRange countRange = IntRange.One;

	public float choiceChance = 1f;

	public float skipChance;

	public List<PawnInventoryOption> subOptionsTakeAll;

	public List<PawnInventoryOption> subOptionsChooseOne;

	public IEnumerable<Thing> GenerateThings()
	{
		if (Rand.Value < skipChance)
		{
			yield break;
		}
		if (thingDef != null && countRange.max > 0)
		{
			Thing thing = ThingMaker.MakeThing(thingDef);
			thing.stackCount = countRange.RandomInRange;
			yield return thing;
		}
		if (subOptionsTakeAll != null)
		{
			foreach (PawnInventoryOption item in subOptionsTakeAll)
			{
				foreach (Thing item2 in item.GenerateThings())
				{
					yield return item2;
				}
			}
		}
		if (subOptionsChooseOne == null)
		{
			yield break;
		}
		PawnInventoryOption pawnInventoryOption = subOptionsChooseOne.RandomElementByWeight((PawnInventoryOption o) => o.choiceChance);
		foreach (Thing item3 in pawnInventoryOption.GenerateThings())
		{
			yield return item3;
		}
	}
}
