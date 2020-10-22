using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_AncientTemple : SymbolResolver
	{
		private const float MechanoidsChance = 0.65f;

		private static readonly IntRange MechanoidCountRange = new IntRange(1, 5);

		private static readonly IntRange HivesCountRange = new IntRange(1, 2);

		private static readonly IntVec2 MinSizeForShrines = new IntVec2(4, 3);

		public override void Resolve(ResolveParams rp)
		{
			List<Thing> list = ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate();
			for (int i = 0; i < list.Count; i++)
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingToSpawn = list[i];
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
			if (!Find.Storyteller.difficultyValues.peacefulTemples)
			{
				if (Rand.Chance(0.65f))
				{
					ResolveParams resolveParams2 = rp;
					resolveParams2.mechanoidsCount = rp.mechanoidsCount ?? MechanoidCountRange.RandomInRange;
					BaseGen.symbolStack.Push("randomMechanoidGroup", resolveParams2);
				}
				else
				{
					ResolveParams resolveParams3 = rp;
					resolveParams3.hivesCount = rp.hivesCount ?? HivesCountRange.RandomInRange;
					BaseGen.symbolStack.Push("hives", resolveParams3);
				}
			}
			if (rp.rect.Width >= MinSizeForShrines.x && rp.rect.Height >= MinSizeForShrines.z)
			{
				BaseGen.symbolStack.Push("ancientShrinesGroup", rp);
			}
		}
	}
}
