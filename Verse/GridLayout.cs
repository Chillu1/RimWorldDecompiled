using UnityEngine;

namespace Verse
{
	public class GridLayout
	{
		public Rect container;

		private int cols;

		private float outerPadding;

		private float innerPadding;

		private float colStride;

		private float rowStride;

		private float colWidth;

		private float rowHeight;

		public GridLayout(Rect container, int cols = 1, int rows = 1, float outerPadding = 4f, float innerPadding = 4f)
		{
			this.container = new Rect(container);
			this.cols = cols;
			this.innerPadding = innerPadding;
			this.outerPadding = outerPadding;
			float num = container.width - outerPadding * 2f - (float)(cols - 1) * innerPadding;
			float num2 = container.height - outerPadding * 2f - (float)(rows - 1) * innerPadding;
			colWidth = num / (float)cols;
			rowHeight = num2 / (float)rows;
			colStride = colWidth + innerPadding;
			rowStride = rowHeight + innerPadding;
		}

		public GridLayout(float colWidth, float rowHeight, int cols, int rows, float outerPadding = 4f, float innerPadding = 4f)
		{
			this.colWidth = colWidth;
			this.rowHeight = rowHeight;
			this.cols = cols;
			this.innerPadding = innerPadding;
			this.outerPadding = outerPadding;
			colStride = colWidth + innerPadding;
			rowStride = rowHeight + innerPadding;
			container = new Rect(0f, 0f, outerPadding * 2f + colWidth * (float)cols + innerPadding * (float)cols - 1f, outerPadding * 2f + rowHeight * (float)rows + innerPadding * (float)rows - 1f);
		}

		public Rect GetCellRectByIndex(int index, int colspan = 1, int rowspan = 1)
		{
			int col = index % cols;
			int row = index / cols;
			return GetCellRect(col, row, colspan, rowspan);
		}

		public Rect GetCellRect(int col, int row, int colspan = 1, int rowspan = 1)
		{
			return new Rect(Mathf.Floor(container.x + outerPadding + (float)col * colStride), Mathf.Floor(container.y + outerPadding + (float)row * rowStride), Mathf.Ceil(colWidth) * (float)colspan + innerPadding * (float)(colspan - 1), Mathf.Ceil(rowHeight) * (float)rowspan + innerPadding * (float)(rowspan - 1));
		}
	}
}
