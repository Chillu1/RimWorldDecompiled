using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Filth : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			if (base.CanResolve(rp) && rp.filthDef != null)
			{
				return rp.filthDensity.HasValue;
			}
			return false;
		}

		public override void Resolve(ResolveParams rp)
		{
			foreach (IntVec3 item in rp.rect)
			{
				if (CanPlaceFilth(item, rp))
				{
					float num;
					for (num = rp.filthDensity.Value.RandomInRange; num > 1f; num -= 1f)
					{
						FilthMaker.TryMakeFilth(item, BaseGen.globalSettings.map, rp.filthDef);
					}
					if (Rand.Chance(num))
					{
						FilthMaker.TryMakeFilth(item, BaseGen.globalSettings.map, rp.filthDef);
					}
				}
			}
		}

		private bool CanPlaceFilth(IntVec3 cell, ResolveParams rp)
		{
			if (rp.ignoreDoorways.HasValue && rp.ignoreDoorways.Value && cell.GetDoor(BaseGen.globalSettings.map) != null)
			{
				return false;
			}
			return true;
		}
	}
}
