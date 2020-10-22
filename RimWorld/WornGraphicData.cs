using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WornGraphicData
	{
		public bool renderUtilityAsPack;

		public WornGraphicDirectionData north;

		public WornGraphicDirectionData south;

		public WornGraphicDirectionData east;

		public WornGraphicDirectionData west;

		public WornGraphicBodyTypeData male;

		public WornGraphicBodyTypeData female;

		public WornGraphicBodyTypeData thin;

		public WornGraphicBodyTypeData hulk;

		public WornGraphicBodyTypeData fat;

		public Vector2 BeltOffsetAt(Rot4 facing, BodyTypeDef bodyType)
		{
			WornGraphicDirectionData wornGraphicDirectionData = default(WornGraphicDirectionData);
			switch (facing.AsInt)
			{
			case 0:
				wornGraphicDirectionData = north;
				break;
			case 1:
				wornGraphicDirectionData = east;
				break;
			case 2:
				wornGraphicDirectionData = south;
				break;
			case 3:
				wornGraphicDirectionData = west;
				break;
			}
			Vector2 offset = wornGraphicDirectionData.offset;
			if (bodyType == BodyTypeDefOf.Male)
			{
				offset += wornGraphicDirectionData.male.offset;
			}
			else if (bodyType == BodyTypeDefOf.Female)
			{
				offset += wornGraphicDirectionData.female.offset;
			}
			else if (bodyType == BodyTypeDefOf.Thin)
			{
				offset += wornGraphicDirectionData.thin.offset;
			}
			else if (bodyType == BodyTypeDefOf.Hulk)
			{
				offset += wornGraphicDirectionData.hulk.offset;
			}
			else if (bodyType == BodyTypeDefOf.Fat)
			{
				offset += wornGraphicDirectionData.fat.offset;
			}
			return offset;
		}

		public Vector2 BeltScaleAt(BodyTypeDef bodyType)
		{
			Vector2 result = Vector2.one;
			if (bodyType == BodyTypeDefOf.Male)
			{
				result = male.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Female)
			{
				result = female.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Thin)
			{
				result = thin.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Hulk)
			{
				result = hulk.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Fat)
			{
				result = fat.Scale;
			}
			return result;
		}
	}
}
