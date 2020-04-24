using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientShrinesGroup : SymbolResolver
	{
		public static readonly IntVec2 StandardAncientShrineSize = new IntVec2(4, 3);

		private const int MaxNumCaskets = 6;

		private const float SkipShrineChance = 0.25f;

		public const int MarginCells = -1;

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			int num = (rp.rect.Width + Mathf.Max(-1, 0)) / (StandardAncientShrineSize.x + -1);
			int num2 = (rp.rect.Height + Mathf.Max(-1, 0)) / (StandardAncientShrineSize.z + -1);
			IntVec3 bottomLeft = rp.rect.BottomLeft;
			PodContentsType? podContentsType = rp.podContentsType;
			if (!podContentsType.HasValue)
			{
				float value = Rand.Value;
				podContentsType = ((value < 0.5f) ? null : ((value < 0.7f) ? new PodContentsType?(PodContentsType.Slave) : new PodContentsType?(PodContentsType.AncientHostile)));
			}
			int value2 = rp.ancientCryptosleepCasketGroupID ?? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					if (!Rand.Chance(0.25f))
					{
						if (num3 >= 6)
						{
							break;
						}
						CellRect rect = new CellRect(bottomLeft.x + j * (StandardAncientShrineSize.x + -1), bottomLeft.z + i * (StandardAncientShrineSize.z + -1), StandardAncientShrineSize.x, StandardAncientShrineSize.z);
						if (rect.FullyContainedWithin(rp.rect) && ThingUtility.InteractionCellWhenAt(center: new IntVec3(rect.minX + rect.Width / 2 - 1, 0, rect.minZ + rect.Height / 2), def: ThingDefOf.AncientCryptosleepCasket, rot: Rot4.East, map: map).Standable(map))
						{
							ResolveParams resolveParams = rp;
							resolveParams.rect = rect;
							resolveParams.ancientCryptosleepCasketGroupID = value2;
							resolveParams.podContentsType = podContentsType;
							BaseGen.symbolStack.Push("ancientShrine", resolveParams);
							num3++;
						}
					}
				}
			}
		}
	}
}
