using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class GameCondition_Planetkiller : GameCondition
	{
		private const int SoundDuration = 179;

		private const int FadeDuration = 90;

		private static readonly Color FadeColor = Color.white;

		public override string TooltipString
		{
			get
			{
				Vector2 location = (Find.CurrentMap == null) ? default(Vector2) : Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
				return (string)(def.LabelCap + "\n" + "\n" + Description + ("\n" + "ImpactDate".Translate().CapitalizeFirst() + ": " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(startTick + base.Duration), location))) + ("\n" + "TimeLeft".Translate().CapitalizeFirst() + ": " + base.TicksLeft.ToStringTicksToPeriod());
			}
		}

		public override void GameConditionTick()
		{
			base.GameConditionTick();
			if (base.TicksLeft <= 179)
			{
				Find.ActiveLesson.Deactivate();
				if (base.TicksLeft == 179)
				{
					SoundDefOf.PlanetkillerImpact.PlayOneShotOnCamera();
				}
				if (base.TicksLeft == 90)
				{
					ScreenFader.StartFade(FadeColor, 1f);
				}
			}
		}

		public override void End()
		{
			base.End();
			Impact();
		}

		private void Impact()
		{
			ScreenFader.SetColor(Color.clear);
			GenGameEnd.EndGameDialogMessage("GameOverPlanetkillerImpact".Translate(Find.World.info.name), allowKeepPlaying: false, FadeColor);
		}
	}
}
