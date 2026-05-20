using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class CaravanMergeUtility
{
	private static readonly Texture2D MergeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/MergeCaravans");

	private static List<Caravan> tmpSelectedPlayerCaravans = new List<Caravan>();

	private static List<Caravan> tmpCaravansOnSameTile = new List<Caravan>();

	private static List<Caravan> tmpShuttleCaravans = new List<Caravan>();

	public static bool ShouldShowMergeCommand
	{
		get
		{
			if (!CanMergeAnySelectedCaravans)
			{
				return AnySelectedCaravanCloseToAnyOtherMergeableCaravan;
			}
			return true;
		}
	}

	public static bool CanMergeAnySelectedCaravans
	{
		get
		{
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				if (!(selectedObjects[i] is Caravan { IsPlayerControlled: not false } caravan))
				{
					continue;
				}
				for (int j = i + 1; j < selectedObjects.Count; j++)
				{
					if (selectedObjects[j] is Caravan { IsPlayerControlled: not false } caravan2 && CloseToEachOther(caravan, caravan2))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static bool AnySelectedCaravanCloseToAnyOtherMergeableCaravan
	{
		get
		{
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				if (!(selectedObjects[i] is Caravan { IsPlayerControlled: not false } caravan))
				{
					continue;
				}
				for (int j = 0; j < caravans.Count; j++)
				{
					Caravan caravan2 = caravans[j];
					if (caravan2 != caravan && caravan2.IsPlayerControlled && CloseToEachOther(caravan, caravan2))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static Command MergeCommand(Caravan caravan)
	{
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandMergeCaravans".Translate();
		command_Action.defaultDesc = "CommandMergeCaravansDesc".Translate();
		command_Action.icon = MergeCommandTex;
		command_Action.groupable = true;
		command_Action.action = delegate
		{
			if (Find.WorldSelector.FirstSelectedObject == caravan)
			{
				TryMergeSelectedCaravans();
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
		};
		if (!CanMergeAnySelectedCaravans)
		{
			command_Action.Disable("CommandMergeCaravansFailCaravansNotSelected".Translate());
		}
		return command_Action;
	}

	public static void TryMergeSelectedCaravans()
	{
		tmpSelectedPlayerCaravans.Clear();
		List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
		for (int i = 0; i < selectedObjects.Count; i++)
		{
			if (selectedObjects[i] is Caravan { IsPlayerControlled: not false } caravan)
			{
				tmpSelectedPlayerCaravans.Add(caravan);
			}
		}
		while (tmpSelectedPlayerCaravans.Any())
		{
			Caravan caravan2 = tmpSelectedPlayerCaravans[0];
			tmpSelectedPlayerCaravans.RemoveAt(0);
			tmpCaravansOnSameTile.Clear();
			tmpCaravansOnSameTile.Add(caravan2);
			tmpShuttleCaravans.Clear();
			if (ModsConfig.OdysseyActive && caravan2.Shuttle != null)
			{
				tmpShuttleCaravans.Add(caravan2);
			}
			for (int num = tmpSelectedPlayerCaravans.Count - 1; num >= 0; num--)
			{
				if (CloseToEachOther(tmpSelectedPlayerCaravans[num], caravan2))
				{
					if (ModsConfig.OdysseyActive && tmpSelectedPlayerCaravans[num].Shuttle != null)
					{
						tmpShuttleCaravans.Add(tmpSelectedPlayerCaravans[num]);
					}
					tmpCaravansOnSameTile.Add(tmpSelectedPlayerCaravans[num]);
					tmpSelectedPlayerCaravans.RemoveAt(num);
				}
			}
			if (tmpCaravansOnSameTile.Count < 2)
			{
				continue;
			}
			if (tmpShuttleCaravans.Count >= 2)
			{
				string text = (from caravan3 in tmpShuttleCaravans.Skip(1)
					select caravan3.Shuttle.LabelCap).ToLineList("  - ");
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmMergeShuttles".Translate(text), delegate
				{
					MergeCaravans(tmpCaravansOnSameTile);
				}));
			}
			else
			{
				MergeCaravans(tmpCaravansOnSameTile);
			}
		}
	}

	private static bool CloseToEachOther(Caravan c1, Caravan c2)
	{
		if (c1.Tile == c2.Tile)
		{
			return true;
		}
		Vector3 drawPos = c1.DrawPos;
		Vector3 drawPos2 = c2.DrawPos;
		float num = Find.WorldGrid.AverageTileSize * 0.5f;
		if ((drawPos - drawPos2).sqrMagnitude < num * num)
		{
			return true;
		}
		return false;
	}

	private static void MergeCaravans(List<Caravan> caravans)
	{
		bool flag = false;
		Caravan caravan = caravans.MaxBy((Caravan x) => x.PawnsListForReading.Count);
		for (int num = 0; num < caravans.Count; num++)
		{
			Caravan caravan2 = caravans[num];
			if (caravan2 == caravan)
			{
				continue;
			}
			if (ModsConfig.OdysseyActive && caravan2.Shuttle != null)
			{
				if (!flag)
				{
					flag = true;
				}
				else
				{
					Building_PassengerShuttle shuttle = caravan2.Shuttle;
					caravan2.GetDirectlyHeldThings().Remove(shuttle);
					caravan2.Shuttle.Destroy();
				}
			}
			caravan2.pawns.TryTransferAllToContainer(caravan.pawns);
			caravan2.Destroy();
		}
		caravan.hasShuttleDirty = true;
		caravan.Notify_Merged(caravans);
	}
}
