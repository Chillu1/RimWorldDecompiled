using System.Collections.Generic;
using RimWorld;

namespace Verse;

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
			if (desList[i] is T result)
			{
				return result;
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
		desList.Add(new Designator_DeconstructConduit());
		desList.Add(new Designator_Uninstall());
		desList.Add(new Designator_Haul());
		desList.Add(new Designator_Hunt());
		desList.Add(new Designator_Slaughter());
		desList.Add(new Designator_Tame());
		desList.Add(new Designator_PlantsCut());
		desList.Add(new Designator_PlantsHarvest());
		desList.Add(new Designator_PlantsHarvestWood());
		desList.Add(new Designator_Mine());
		desList.Add(new Designator_MineVein());
		desList.Add(new Designator_Strip());
		desList.Add(new Designator_Open());
		desList.Add(new Designator_EjectFuel());
		desList.Add(new Designator_SmoothSurface());
		desList.Add(new Designator_SmoothFloors());
		desList.Add(new Designator_SmoothWalls());
		desList.Add(new Designator_ReleaseAnimalToWild());
		desList.Add(new Designator_ExtractTree());
		desList.Add(new Designator_RemovePaint());
		if (ModsConfig.BiotechActive)
		{
			desList.Add(new Designator_MechControlGroup());
			desList.Add(new Designator_Adopt());
		}
		desList.Add(new Designator_FillIn());
		desList.Add(new Designator_ExtractSkull());
		desList.RemoveAll((Designator des) => !Current.Game.Rules.DesignatorAllowed(des));
	}
}
