using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorder : IExposable
	{
		public HistoryAutoRecorderDef def;

		public List<float> records = new List<float>();

		public void Tick()
		{
			if (Find.TickManager.TicksGame % def.recordTicksFrequency == 0 || !records.Any())
			{
				float item = def.Worker.PullRecord();
				records.Add(item);
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			byte[] arr = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				arr = RecordsToBytes();
			}
			DataExposeUtility.ByteArray(ref arr, "records");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				SetRecordsFromBytes(arr);
			}
		}

		private byte[] RecordsToBytes()
		{
			byte[] array = new byte[records.Count * 4];
			for (int i = 0; i < records.Count; i++)
			{
				byte[] bytes = BitConverter.GetBytes(records[i]);
				for (int j = 0; j < 4; j++)
				{
					array[i * 4 + j] = bytes[j];
				}
			}
			return array;
		}

		private void SetRecordsFromBytes(byte[] bytes)
		{
			int num = bytes.Length / 4;
			records.Clear();
			for (int i = 0; i < num; i++)
			{
				float item = BitConverter.ToSingle(bytes, i * 4);
				records.Add(item);
			}
		}
	}
}
