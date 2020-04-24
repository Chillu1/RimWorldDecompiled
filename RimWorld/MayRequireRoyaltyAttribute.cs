using Verse;

namespace RimWorld
{
	public class MayRequireRoyaltyAttribute : MayRequireAttribute
	{
		public MayRequireRoyaltyAttribute()
			: base(ModContentPack.RoyaltyModPackageId)
		{
		}
	}
}
