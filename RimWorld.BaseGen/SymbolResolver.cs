using Verse;

namespace RimWorld.BaseGen
{
	public abstract class SymbolResolver
	{
		public IntVec2 minRectSize = IntVec2.One;

		public float selectionWeight = 1f;

		public virtual bool CanResolve(ResolveParams rp)
		{
			if (rp.rect.Width >= minRectSize.x)
			{
				return rp.rect.Height >= minRectSize.z;
			}
			return false;
		}

		public abstract void Resolve(ResolveParams rp);
	}
}
