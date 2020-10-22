using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class CompMeditationFocus : CompStatOffsetBase
	{
		public new CompProperties_MeditationFocus Props => (CompProperties_MeditationFocus)props;

		public override float GetStatOffset(Pawn pawn = null)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Meditation foci are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657117);
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < Props.offsets.Count; i++)
			{
				if (Props.offsets[i].CanApply(parent, pawn))
				{
					num += Props.offsets[i].GetOffset(parent, pawn);
				}
			}
			return num;
		}

		public override IEnumerable<string> GetExplanation()
		{
			for (int i = 0; i < Props.offsets.Count; i++)
			{
				string explanation = Props.offsets[i].GetExplanation(parent);
				if (!explanation.NullOrEmpty())
				{
					yield return explanation;
				}
			}
		}

		public bool CanPawnUse(Pawn pawn)
		{
			for (int i = 0; i < Props.focusTypes.Count; i++)
			{
				if (Props.focusTypes[i].CanPawnUse(pawn))
				{
					return true;
				}
			}
			return false;
		}

		public bool WillBeAffectedBy(ThingDef def, Faction faction, IntVec3 pos, Rot4 rotation)
		{
			CellRect cellRect = GenAdj.OccupiedRect(pos, rotation, def.size);
			foreach (FocusStrengthOffset offset in Props.offsets)
			{
				FocusStrengthOffset_ArtificialBuildings focusStrengthOffset_ArtificialBuildings;
				if ((focusStrengthOffset_ArtificialBuildings = offset as FocusStrengthOffset_ArtificialBuildings) != null && MeditationUtility.CountsAsArtificialBuilding(def, faction) && cellRect.ClosestCellTo(parent.Position).DistanceTo(parent.Position) <= focusStrengthOffset_ArtificialBuildings.radius)
				{
					return true;
				}
			}
			return false;
		}

		public override string CompInspectStringExtra()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (base.LastUser != null)
			{
				stringBuilder.Append("UserMeditationFocusStrength".Translate(base.LastUser.Named("LASTUSER")) + ": " + Props.statDef.ValueToString(parent.GetStatValueForPawn(Props.statDef, base.LastUser)));
			}
			for (int i = 0; i < Props.offsets.Count; i++)
			{
				string text = Props.offsets[i].InspectStringExtra(parent);
				if (!text.NullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (ModsConfig.RoyaltyActive)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Meditation, "MeditationFocuses".Translate(), Props.focusTypes.Select((MeditationFocusDef f) => f.label).ToCommaList().CapitalizeFirst(), "MeditationFocusesDesc".Translate(), 99995);
			}
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			base.PostDrawExtraSelectionOverlays();
			if (ModsConfig.RoyaltyActive)
			{
				for (int i = 0; i < Props.offsets.Count; i++)
				{
					Props.offsets[i].PostDrawExtraSelectionOverlays(parent, base.LastUser);
				}
			}
		}
	}
}
