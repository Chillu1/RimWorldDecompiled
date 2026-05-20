using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Command_SetBedOwnerType : Command
	{
		private Building_Bed bed;

		private static readonly Texture2D ForColonistsTex = ContentFinder<Texture2D>.Get("UI/Commands/ForColonists");

		private static readonly Texture2D ForSlavesTex = ContentFinder<Texture2D>.Get("UI/Commands/ForSlaves");

		private static readonly Texture2D ForPrisonersTex = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");

		public Command_SetBedOwnerType(Building_Bed bed)
		{
			this.bed = bed;
			switch (bed.ForOwnerType)
			{
			case BedOwnerType.Colonist:
				defaultLabel = "CommandBedSetForColonistsLabel".Translate();
				icon = ForColonistsTex;
				break;
			case BedOwnerType.Slave:
				defaultLabel = "CommandBedSetForSlavesLabel".Translate();
				icon = ForSlavesTex;
				break;
			case BedOwnerType.Prisoner:
				defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
				icon = ForPrisonersTex;
				break;
			default:
				Log.Error($"Unknown owner type selected for bed: {bed.ForOwnerType}");
				break;
			}
			defaultDesc = "CommandBedSetForOwnerTypeDesc".Translate();
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("CommandBedSetForColonistsLabel".Translate(), delegate
			{
				bed.SetBedOwnerTypeByInterface(BedOwnerType.Colonist);
			}, ForColonistsTex, Color.white));
			list.Add(new FloatMenuOption("CommandBedSetForPrisonersLabel".Translate(), delegate
			{
				if (!Building_Bed.RoomCanBePrisonCell(bed.GetRoom()) && !bed.ForPrisoners)
				{
					Messages.Message("CommandBedSetForPrisonersFailOutdoors".Translate(), bed, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					bed.SetBedOwnerTypeByInterface(BedOwnerType.Prisoner);
				}
			}, ForPrisonersTex, Color.white));
			list.Add(new FloatMenuOption("CommandBedSetForSlavesLabel".Translate(), delegate
			{
				bed.SetBedOwnerTypeByInterface(BedOwnerType.Slave);
			}, ForSlavesTex, Color.white));
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
