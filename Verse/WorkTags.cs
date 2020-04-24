using System;

namespace Verse
{
	[Flags]
	public enum WorkTags
	{
		None = 0x0,
		ManualDumb = 0x2,
		ManualSkilled = 0x4,
		Violent = 0x8,
		Caring = 0x10,
		Social = 0x20,
		Commoner = 0x40,
		Intellectual = 0x80,
		Animals = 0x100,
		Artistic = 0x200,
		Crafting = 0x400,
		Cooking = 0x800,
		Firefighting = 0x1000,
		Cleaning = 0x2000,
		Hauling = 0x4000,
		PlantWork = 0x8000,
		Mining = 0x10000,
		Hunting = 0x20000
	}
}
