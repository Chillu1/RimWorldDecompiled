using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderGroupDef : Def
	{
		public bool useFixedScale;

		public Vector2 fixedScale;

		public bool integersOnly;

		public bool onlyPositiveValues = true;

		public bool devModeOnly;

		public List<HistoryAutoRecorderDef> historyAutoRecorderDefs = new List<HistoryAutoRecorderDef>();

		public static HistoryAutoRecorderGroupDef Named(string defName)
		{
			return DefDatabase<HistoryAutoRecorderGroupDef>.GetNamed(defName);
		}
	}
}
