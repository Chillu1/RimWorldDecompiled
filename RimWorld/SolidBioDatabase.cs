using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SolidBioDatabase
	{
		public static List<PawnBio> allBios = new List<PawnBio>();

		public static void Clear()
		{
			allBios.Clear();
		}

		public static void LoadAllBios()
		{
			foreach (PawnBio item in DirectXmlLoader.LoadXmlDataInResourcesFolder<PawnBio>("Backstories/Solid"))
			{
				item.name.ResolveMissingPieces();
				if (item.childhood == null || item.adulthood == null)
				{
					PawnNameDatabaseSolid.AddPlayerContentName(item.name, item.gender);
				}
				else
				{
					item.PostLoad();
					item.ResolveReferences();
					foreach (string item2 in item.ConfigErrors())
					{
						Log.Error(item2);
					}
					allBios.Add(item);
					item.childhood.shuffleable = false;
					item.childhood.slot = BackstorySlot.Childhood;
					item.adulthood.shuffleable = false;
					item.adulthood.slot = BackstorySlot.Adulthood;
					BackstoryDatabase.AddBackstory(item.childhood);
					BackstoryDatabase.AddBackstory(item.adulthood);
				}
			}
		}
	}
}
