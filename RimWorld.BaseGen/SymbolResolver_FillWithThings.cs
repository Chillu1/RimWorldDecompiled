using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_FillWithThings : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (rp.singleThingToSpawn != null)
		{
			return false;
		}
		if (rp.singleThingDef != null)
		{
			Rot4 rot = rp.thingRot ?? Rot4.North;
			IntVec3 center = IntVec3.Zero;
			IntVec2 size = rp.singleThingDef.size;
			GenAdj.AdjustForRotation(ref center, ref size, rot);
			if (rp.rect.Width < size.x || rp.rect.Height < size.z)
			{
				return false;
			}
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		ThingDef thingDef = rp.singleThingDef ?? ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsWeapon || x.IsMedicine || x.IsDrug).RandomElement();
		Rot4 rot = rp.thingRot ?? Rot4.North;
		IntVec3 center = IntVec3.Zero;
		IntVec2 size = thingDef.size;
		int num = rp.fillWithThingsPadding.GetValueOrDefault();
		if (num < 0)
		{
			num = 0;
		}
		GenAdj.AdjustForRotation(ref center, ref size, rot);
		if (size.x <= 0 || size.z <= 0)
		{
			Log.Error("Thing has 0 size.");
			return;
		}
		for (int num2 = rp.rect.minX; num2 <= rp.rect.maxX - size.x + 1; num2 += size.x + num)
		{
			for (int num3 = rp.rect.minZ; num3 <= rp.rect.maxZ - size.z + 1; num3 += size.z + num)
			{
				ResolveParams resolveParams = rp;
				resolveParams.rect = new CellRect(num2, num3, size.x, size.z);
				resolveParams.singleThingDef = thingDef;
				resolveParams.thingRot = rot;
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
		}
		BaseGen.symbolStack.Push("clear", rp);
	}
}
