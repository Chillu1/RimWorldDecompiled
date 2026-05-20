using System;

namespace RimWorld;

[Flags]
public enum FoodTypeFlags
{
	None = 0,
	VegetableOrFruit = 1,
	Meat = 2,
	Fluid = 4,
	Corpse = 8,
	Seed = 0x10,
	AnimalProduct = 0x20,
	Plant = 0x40,
	Tree = 0x80,
	Meal = 0x100,
	Processed = 0x200,
	Liquor = 0x400,
	Kibble = 0x800,
	Fungus = 0x1001,
	VegetarianAnimal = 0x1F11,
	VegetarianRoughAnimal = 0x1F51,
	CarnivoreAnimal = 0xB0A,
	CarnivoreAnimalStrict = 0xA,
	OmnivoreAnimal = 0x1F1B,
	OmnivoreRoughAnimal = 0x1F5B,
	DendrovoreAnimal = 0x1A91,
	OvivoreAnimal = 0xB20,
	OmnivoreHuman = 0x1F3F
}
