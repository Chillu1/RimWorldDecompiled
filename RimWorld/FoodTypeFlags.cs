using System;

namespace RimWorld
{
	[Flags]
	public enum FoodTypeFlags
	{
		None = 0x0,
		VegetableOrFruit = 0x1,
		Meat = 0x2,
		Fluid = 0x4,
		Corpse = 0x8,
		Seed = 0x10,
		AnimalProduct = 0x20,
		Plant = 0x40,
		Tree = 0x80,
		Meal = 0x100,
		Processed = 0x200,
		Liquor = 0x400,
		Kibble = 0x800,
		VegetarianAnimal = 0xF11,
		VegetarianRoughAnimal = 0xF51,
		CarnivoreAnimal = 0xB0A,
		CarnivoreAnimalStrict = 0xA,
		OmnivoreAnimal = 0xF1B,
		OmnivoreRoughAnimal = 0xF5B,
		DendrovoreAnimal = 0xA91,
		OvivoreAnimal = 0xB20,
		OmnivoreHuman = 0xF3F
	}
}
