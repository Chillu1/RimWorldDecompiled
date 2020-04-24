using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class PawnNameDatabaseShuffled
	{
		private static Dictionary<PawnNameCategory, NameBank> banks;

		static PawnNameDatabaseShuffled()
		{
			banks = new Dictionary<PawnNameCategory, NameBank>();
			foreach (PawnNameCategory value in Enum.GetValues(typeof(PawnNameCategory)))
			{
				if (value != 0)
				{
					banks.Add(value, new NameBank(value));
				}
			}
			NameBank nameBank = BankOf(PawnNameCategory.HumanStandard);
			nameBank.AddNamesFromFile(PawnNameSlot.First, Gender.Male, "First_Male");
			nameBank.AddNamesFromFile(PawnNameSlot.First, Gender.Female, "First_Female");
			nameBank.AddNamesFromFile(PawnNameSlot.Nick, Gender.Male, "Nick_Male");
			nameBank.AddNamesFromFile(PawnNameSlot.Nick, Gender.Female, "Nick_Female");
			nameBank.AddNamesFromFile(PawnNameSlot.Nick, Gender.None, "Nick_Unisex");
			nameBank.AddNamesFromFile(PawnNameSlot.Last, Gender.None, "Last");
			foreach (NameBank value2 in banks.Values)
			{
				value2.ErrorCheck();
			}
		}

		public static NameBank BankOf(PawnNameCategory category)
		{
			return banks[category];
		}
	}
}
