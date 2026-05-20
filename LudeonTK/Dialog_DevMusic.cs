using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace LudeonTK;

[StaticConstructorOnStartup]
public class Dialog_DevMusic : Window_Dev
{
	private Vector2 windowPosition;

	private const string Title = "Music Debugger";

	private const float ButtonHeight = 30f;

	public override bool IsDebug => true;

	protected override float Margin => 4f;

	public override Vector2 InitialSize => new Vector2(275f, 360f);

	public Dialog_DevMusic()
	{
		draggable = true;
		focusWhenOpened = false;
		drawShadow = false;
		closeOnAccept = false;
		closeOnCancel = false;
		preventCameraMotion = false;
		drawInScreenshotMode = false;
		windowPosition = Prefs.DevPalettePosition;
		onlyDrawInDevMode = true;
		doCloseX = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		Rect rect = new Rect(inRect.x, inRect.y, inRect.width, 24f);
		DevGUI.Label(rect, "Music Debugger");
		float y = rect.height + 6f;
		Text.Font = GameFont.Tiny;
		MusicManagerPlay musicManagerPlay = Find.MusicManagerPlay;
		PrintLabel($"State: {musicManagerPlay.State}", inRect, ref y);
		PrintLabel("", inRect, ref y);
		PrintLabel("Song: " + (musicManagerPlay.IsPlaying ? $"{musicManagerPlay.CurrentSong.defName} ({musicManagerPlay.SongTime:0}/{musicManagerPlay.SongDuration:0}s)" : "NA"), inRect, ref y);
		if (musicManagerPlay.State == MusicManagerPlay.MusicManagerState.Fadeout)
		{
			PrintLabel(string.Format("Fadeout Progress: {0} ({1:0.0}s)", musicManagerPlay.FadeoutPercent.ToStringPercent("0"), musicManagerPlay.FadeoutDuration), inRect, ref y);
		}
		else if (musicManagerPlay.IsPlaying)
		{
			PrintLabel("Volume: " + musicManagerPlay.CurSanitizedVolume.ToStringPercent("0"), inRect, ref y);
		}
		else
		{
			PrintLabel("Next Song: " + ((!musicManagerPlay.IsPlaying && musicManagerPlay.NextSongTimer > 0f) ? $"{musicManagerPlay.NextSongTimer:0.0}s" : "NA"), inRect, ref y);
		}
		PrintLabel("Pause Vol Factor: " + musicManagerPlay.PausedVolumeFactor.ToStringPercent("0"), inRect, ref y);
		Rect rect2 = inRect;
		rect2.y = y;
		rect2.height = 18f;
		rect2.xMin = rect2.width * 0.65f;
		rect2.xMax -= 2f;
		PrintLabel($"Danger Mode: {musicManagerPlay.DangerMusicMode}", inRect, ref y);
		if (DevGUI.ButtonText(rect2, musicManagerPlay.OverrideDangerMode ? "Override On" : "Override Off"))
		{
			musicManagerPlay.OverrideDangerMode = !musicManagerPlay.OverrideDangerMode;
		}
		PrintLabel("", inRect, ref y);
		PrintLabel("Source Transition: " + ((musicManagerPlay.TriggeredTransition != null) ? musicManagerPlay.TriggeredTransition.def.defName : "NA"), inRect, ref y);
		PrintLabel("Current Sequence: " + ((musicManagerPlay.MusicSequenceWorker != null) ? musicManagerPlay.MusicSequenceWorker.def.defName : "NA"), inRect, ref y);
		MusicSequenceWorker musicSequenceWorker = musicManagerPlay.MusicSequenceWorker;
		if (musicSequenceWorker != null)
		{
			PrintLabel("   - Next Sequence: " + ((musicSequenceWorker.def.nextSequence != null) ? musicSequenceWorker.def.nextSequence.defName : "NA"), inRect, ref y);
			PrintLabel($"   - Loop: {musicSequenceWorker.ShouldLoop()}", inRect, ref y);
			PrintLabel($"   - Can Transition: {musicSequenceWorker.ShouldTransition()}", inRect, ref y);
			PrintLabel($"   - Interruptible: {musicSequenceWorker.CanBeInterrupted()}", inRect, ref y);
			PrintLabel($"   - Times Looped: {musicSequenceWorker.timesLooped}", inRect, ref y);
		}
		Rect rect3 = inRect;
		rect3.yMin = rect3.yMax - 30f - 2f;
		rect3.yMax -= 2f;
		rect3.xMin = 2f;
		rect3.xMax = inRect.width / 2f - 2f;
		Rect rect4 = inRect;
		rect4.yMin = rect4.yMax - 30f - 2f;
		rect4.yMax -= 2f;
		rect4.xMin = inRect.width / 2f + 2f;
		rect4.xMax -= 2f;
		if (DevGUI.ButtonText(rect3, musicManagerPlay.IsPlaying ? "Stop" : "Start", TextAnchor.MiddleCenter))
		{
			if (musicManagerPlay.IsPlaying)
			{
				musicManagerPlay.Stop();
			}
			else
			{
				musicManagerPlay.ScheduleNewSong();
			}
		}
		if (musicManagerPlay.MusicSequenceWorker == null)
		{
			if (!DevGUI.ButtonText(rect4, "Trigger Transition", TextAnchor.MiddleCenter))
			{
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (MusicTransitionDef allDef in DefDatabase<MusicTransitionDef>.AllDefs)
			{
				MusicTransitionDef local = allDef;
				list.Add(new FloatMenuOption(local.defName, delegate
				{
					Find.MusicManagerPlay.ForceTriggerTransition(local);
				}));
			}
			FloatMenu window = new FloatMenu(list, "Select transition");
			Find.WindowStack.Add(window);
		}
		else
		{
			bool flag = musicManagerPlay.MusicSequenceWorker.def.nextSequence != null;
			if (DevGUI.ButtonText(rect4, flag ? "Next Sequence" : "Next Song", TextAnchor.MiddleCenter))
			{
				musicManagerPlay.ForceTriggerNextSongOrSequence();
			}
		}
	}

	private void PrintLabel(string text, Rect container, ref float y)
	{
		DevGUI.Label(new Rect(container.x, y, container.width, 20f), text);
		y += 20f;
	}
}
