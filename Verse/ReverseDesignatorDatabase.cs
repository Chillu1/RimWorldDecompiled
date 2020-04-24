using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class ReverseDesignatorDatabase
	{
		private List<Designator> desList;

		public List<Designator> AllDesignators
		{
			get
			{
				if (desList == null)
				{
					InitDesignators();
				}
				return desList;
			}
		}

		public void Reinit()
		{
			desList = null;
		}

		public T Get<T>() where T : Designator
		{
			if (desList == null)
			{
				InitDesignators();
			}
			for (int i = 0; i < desList.Count; i++)
			{
				T val = desList[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		private void InitDesignators()
		{
			desList = new List<Designator>();
			desList.Add(new Designator_Cancel());
			desList.Add(new Designator_Claim());
			desList.Add(new Designator_Deconstruct());
			desList.Add(new Designator_Uninstall());
			desList.Add(new Designator_Haul());
			desList.Add(new Designator_Hunt());
			desList.Add(new Designator_Slaughter());
			desList.Add(new Designator_Tame());
			desList.Add(new Designator_PlantsCut());
			desList.Add(new Designator_PlantsHarvest());
			desList.Add(new Designator_PlantsHarvestWood());
			desList.Add(new Designator_Mine());
			desList.Add(new Designator_Strip());
			desList.Add(new Designator_Open());
			desList.Add(new Designator_SmoothSurface());
			desList.RemoveAll((Designator des) => !Current.Game.Rules.DesignatorAllowed(des));
		}
	}
}
