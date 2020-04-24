using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class SettlementAbandonUtility
	{
		private static readonly Texture2D AbandonCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/AbandonHome");

		public static Command AbandonCommand(MapParent settlement)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandAbandonHome".Translate();
			command_Action.defaultDesc = "CommandAbandonHomeDesc".Translate();
			command_Action.icon = AbandonCommandTex;
			command_Action.action = delegate
			{
				TryAbandonViaInterface(settlement);
			};
			command_Action.order = 30f;
			if (AllColonistsThere(settlement))
			{
				command_Action.Disable("CommandAbandonHomeFailAllColonistsThere".Translate());
			}
			return command_Action;
		}

		public static bool AllColonistsThere(MapParent settlement)
		{
			if (!CaravanUtility.PlayerHasAnyCaravan())
			{
				return !Find.Maps.Any((Map x) => x.info.parent != settlement && x.mapPawns.FreeColonistsSpawned.Any());
			}
			return false;
		}

		public static void TryAbandonViaInterface(MapParent settlement)
		{
			Map map = settlement.Map;
			if (map == null)
			{
				Abandon(settlement);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<Pawn> source = map.mapPawns.PawnsInFaction(Faction.OfPlayer);
			if (source.Count() != 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				foreach (Pawn item in source.OrderByDescending((Pawn x) => x.IsColonist))
				{
					if (stringBuilder2.Length > 0)
					{
						stringBuilder2.AppendLine();
					}
					stringBuilder2.Append("    " + item.LabelCap);
				}
				stringBuilder.Append("ConfirmAbandonHomeWithColonyPawns".Translate(stringBuilder2));
			}
			PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(map.mapPawns.AllPawns, PawnDiedOrDownedThoughtsKind.Banished, stringBuilder, null, "\n\n" + "ConfirmAbandonHomeNegativeThoughts_Everyone".Translate(), "ConfirmAbandonHomeNegativeThoughts");
			if (stringBuilder.Length == 0)
			{
				Abandon(settlement);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			else
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), delegate
				{
					Abandon(settlement);
				}));
			}
		}

		private static void Abandon(MapParent settlement)
		{
			settlement.Destroy();
			Settlement settlement2 = settlement as Settlement;
			if (settlement2 != null)
			{
				AddAbandonedSettlement(settlement2);
			}
			Find.GameEnder.CheckOrUpdateGameOver();
		}

		private static void AddAbandonedSettlement(Settlement factionBase)
		{
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedSettlement);
			worldObject.Tile = factionBase.Tile;
			worldObject.SetFaction(factionBase.Faction);
			Find.WorldObjects.Add(worldObject);
		}
	}
}
