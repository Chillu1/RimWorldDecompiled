using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanMergeUtility
	{
		private static readonly Texture2D MergeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/MergeCaravans");

		private static List<Caravan> tmpSelectedPlayerCaravans = new List<Caravan>();

		private static List<Caravan> tmpCaravansOnSameTile = new List<Caravan>();

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
					Caravan caravan = selectedObjects[i] as Caravan;
					if (caravan == null || !caravan.IsPlayerControlled)
					{
						continue;
					}
					for (int j = i + 1; j < selectedObjects.Count; j++)
					{
						Caravan caravan2 = selectedObjects[j] as Caravan;
						if (caravan2 != null && caravan2.IsPlayerControlled && CloseToEachOther(caravan, caravan2))
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
					Caravan caravan = selectedObjects[i] as Caravan;
					if (caravan == null || !caravan.IsPlayerControlled)
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
			command_Action.action = delegate
			{
				TryMergeSelectedCaravans();
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
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
				Caravan caravan = selectedObjects[i] as Caravan;
				if (caravan != null && caravan.IsPlayerControlled)
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
				for (int num = tmpSelectedPlayerCaravans.Count - 1; num >= 0; num--)
				{
					if (CloseToEachOther(tmpSelectedPlayerCaravans[num], caravan2))
					{
						tmpCaravansOnSameTile.Add(tmpSelectedPlayerCaravans[num]);
						tmpSelectedPlayerCaravans.RemoveAt(num);
					}
				}
				if (tmpCaravansOnSameTile.Count >= 2)
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
			float num = Find.WorldGrid.averageTileSize * 0.5f;
			if ((drawPos - drawPos2).sqrMagnitude < num * num)
			{
				return true;
			}
			return false;
		}

		private static void MergeCaravans(List<Caravan> caravans)
		{
			Caravan caravan = caravans.MaxBy((Caravan x) => x.PawnsListForReading.Count);
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan2 = caravans[i];
				if (caravan2 != caravan)
				{
					caravan2.pawns.TryTransferAllToContainer(caravan.pawns);
					caravan2.Destroy();
				}
			}
			caravan.Notify_Merged(caravans);
		}
	}
}
