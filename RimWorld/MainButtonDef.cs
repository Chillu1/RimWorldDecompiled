using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainButtonDef : Def
	{
		public Type workerClass = typeof(MainButtonWorker_ToggleTab);

		public Type tabWindowClass;

		public bool buttonVisible = true;

		public int order;

		public KeyCode defaultHotKey;

		public bool canBeTutorDenied = true;

		public bool validWithoutMap;

		public bool minimized;

		public string iconPath;

		[Unsaved(false)]
		public KeyBindingDef hotKey;

		[Unsaved(false)]
		public string cachedTutorTag;

		[Unsaved(false)]
		public string cachedHighlightTagClosed;

		[Unsaved(false)]
		private MainButtonWorker workerInt;

		[Unsaved(false)]
		private MainTabWindow tabWindowInt;

		[Unsaved(false)]
		private string cachedShortenedLabelCap;

		[Unsaved(false)]
		private float cachedLabelCapWidth = -1f;

		[Unsaved(false)]
		private float cachedShortenedLabelCapWidth = -1f;

		[Unsaved(false)]
		private Texture2D icon;

		public const int ButtonHeight = 35;

		public MainButtonWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (MainButtonWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public MainTabWindow TabWindow
		{
			get
			{
				if (tabWindowInt == null && tabWindowClass != null)
				{
					tabWindowInt = (MainTabWindow)Activator.CreateInstance(tabWindowClass);
					tabWindowInt.def = this;
				}
				return tabWindowInt;
			}
		}

		public string ShortenedLabelCap
		{
			get
			{
				if (cachedShortenedLabelCap == null)
				{
					cachedShortenedLabelCap = base.LabelCap.Shorten();
				}
				return cachedShortenedLabelCap;
			}
		}

		public float LabelCapWidth
		{
			get
			{
				if (cachedLabelCapWidth < 0f)
				{
					GameFont font = Text.Font;
					Text.Font = GameFont.Small;
					cachedLabelCapWidth = Text.CalcSize(base.LabelCap).x;
					Text.Font = font;
				}
				return cachedLabelCapWidth;
			}
		}

		public float ShortenedLabelCapWidth
		{
			get
			{
				if (cachedShortenedLabelCapWidth < 0f)
				{
					GameFont font = Text.Font;
					Text.Font = GameFont.Small;
					cachedShortenedLabelCapWidth = Text.CalcSize(ShortenedLabelCap).x;
					Text.Font = font;
				}
				return cachedShortenedLabelCapWidth;
			}
		}

		public Texture2D Icon
		{
			get
			{
				if (icon == null && iconPath != null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return icon;
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			cachedHighlightTagClosed = "MainTab-" + defName + "-Closed";
		}

		public void Notify_SwitchedMap()
		{
			if (tabWindowInt != null)
			{
				Find.WindowStack.TryRemove(tabWindowInt);
				tabWindowInt = null;
			}
		}

		public void Notify_ClearingAllMapsMemory()
		{
			tabWindowInt = null;
		}
	}
}
