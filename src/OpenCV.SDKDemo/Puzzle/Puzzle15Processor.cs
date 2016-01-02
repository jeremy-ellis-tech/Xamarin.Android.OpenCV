using Android.Util;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.SDKDemo.Utilities;
using System;

namespace OpenCV.SDKDemo.Puzzle
{
    public class Puzzle15Processor
    {
        private const int GridSize = 4;
        private const int GridArea = GridSize * GridSize;
        private const int GridEmptyIndex = GridArea - 1;
        private static readonly Scalar GridEmptyColor = new Scalar(0x33, 0x33, 0x33, 0xFF);
        Random random = new Random();

        private int[] _indexes;
        private int[] _textWidths;
        private int[] _textHeights;

        private Mat _rgba15;
        private Mat[] _cells15;
        private bool _showTileNumbers = true;

        private readonly object _lock = new object();

        public Puzzle15Processor()
        {
            _textWidths = new int[GridArea];
            _textHeights = new int[GridArea];
            _indexes = new int[GridArea];
            for (int i = 0; i < GridArea; i++)
            {
                _indexes[i] = i;
            }
        }

        public void PrepareNewGame()
        {
            lock (_lock)
            {
                do
                {
                    Shuffle(_indexes);
                } while (!isPuzzleSolvable());
            }
        }

        private void Shuffle(int[] array)
        {
            for (int i = array.Length; i > 1; i--)
            {
                int temp = array[i - 1];
                int randIx = (int)(random.NextDouble() * i);
                array[i - 1] = array[randIx];
                array[randIx] = temp;
            }
        }

        private bool isPuzzleSolvable()
        {
            int sum = 0;
            for (int i = 0; i < GridArea; i++)
            {
                if (_indexes[i] == GridEmptyIndex)
                    sum += (i / GridSize) + 1;
                else
                {
                    int smaller = 0;
                    for (int j = i + 1; j < GridArea; j++)
                    {
                        if (_indexes[j] < _indexes[i])
                            smaller++;
                    }
                    sum += smaller;
                }
            }
            return sum % 2 == 0;
        }

        private void DrawGrid(int cols, int rows, Mat drawMat)
        {
            for (int i = 1; i < GridSize; i++)
            {
                Imgproc.Line(drawMat, new Point(0, i * rows / GridSize), new Point(cols, i * rows / GridSize), new Scalar(0, 255, 0, 255), 3);
                Imgproc.Line(drawMat, new Point(i * cols / GridSize, 0), new Point(i * cols / GridSize, rows), new Scalar(0, 255, 0, 255), 3);
            }
        }

        internal void ToggleTileNumbers()
        {
            _showTileNumbers = !_showTileNumbers;
        }

        internal void PrepareGameSize(int width, int height)
        {
            lock (_lock)
            {
                _rgba15 = new Mat(height, width, CvType.Cv8uc4);
                _cells15 = new Mat[GridArea];

                for (int i = 0; i < GridSize; i++)
                {
                    for (int j = 0; j < GridSize; j++)
                    {
                        int k = i * GridSize + j;
                        _cells15[k] = _rgba15.Submat(i * height / GridSize, (i + 1) * height / GridSize, j * width / GridSize, (j + 1) * width / GridSize);
                    }
                }

                for (int i = 0; i < GridArea; i++)
                {
                    var s = Imgproc.GetTextSize((i + 1).ToString(), 3/* CV_FONT_HERSHEY_COMPLEX */, 1, 2, null);
                    _textHeights[i] = (int)s.Height;
                    _textWidths[i] = (int)s.Width;
                }
            }
        }

        internal Mat PuzzleFrame(Mat inputPicture)
        {
            lock (_lock)
            {
                Mat[] cells = new Mat[GridArea];
                int rows = inputPicture.Rows();
                int cols = inputPicture.Cols();

                rows = rows - rows % 4;
                cols = cols - cols % 4;

                for (int i = 0; i < GridSize; i++)
                {
                    for (int j = 0; j < GridSize; j++)
                    {
                        int k = i * GridSize + j;
                        cells[k] = inputPicture.Submat(i * inputPicture.Rows() / GridSize,
                            (i + 1) * inputPicture.Rows() / GridSize, j * inputPicture.Cols() / GridSize,
                            (j + 1) * inputPicture.Cols() / GridSize);
                    }
                }

                rows = rows - rows % 4;
                cols = cols - cols % 4;

                // copy shuffled tiles
                for (int i = 0; i < GridArea; i++)
                {
                    int idx = _indexes[i];
                    if (idx == GridEmptyIndex)
                        _cells15[i].SetTo(GridEmptyColor);
                    else
                    {
                        cells[idx].CopyTo(_cells15[i]);
                        if (_showTileNumbers)
                        {
                            Imgproc.PutText(_cells15[i], (1 + idx).ToString(), new Point((cols / GridSize - _textWidths[idx]) / 2,
                                    (rows / GridSize + _textHeights[idx]) / 2), 3/* CV_FONT_HERSHEY_COMPLEX */, 1, new Scalar(255, 0, 0, 255), 2);
                        }
                    }
                }

                for (int i = 0; i < GridArea; i++)
                    cells[i].Release();

                DrawGrid(cols, rows, _rgba15);

                return _rgba15;
            }
        }

        internal void DeliverTouchEvent(int x, int y)
        {
            int rows = _rgba15.Rows();
            int cols = _rgba15.Cols();

            int row = (int)Math.Floor((double)(y * GridSize / rows));
            int col = (int)Math.Floor((double)(x * GridSize / cols));

            if (row < 0 || row >= GridSize || col < 0 || col >= GridSize)
            {
                Log.Error(ActivityTags.Puzzle, "It is not expected to get touch event outside of picture");
                return;
            }

            int idx = row * GridSize + col;
            int idxtoswap = -1;

            // left
            if (idxtoswap < 0 && col > 0)
                if (_indexes[idx - 1] == GridEmptyIndex)
                    idxtoswap = idx - 1;
            // right
            if (idxtoswap < 0 && col < GridSize - 1)
                if (_indexes[idx + 1] == GridEmptyIndex)
                    idxtoswap = idx + 1;
            // top
            if (idxtoswap < 0 && row > 0)
                if (_indexes[idx - GridSize] == GridEmptyIndex)
                    idxtoswap = idx - GridSize;
            // bottom
            if (idxtoswap < 0 && row < GridSize - 1)
                if (_indexes[idx + GridSize] == GridEmptyIndex)
                    idxtoswap = idx + GridSize;

            // swap
            if (idxtoswap >= 0)
            {
                lock (_lock)
                {
                    int touched = _indexes[idx];
                    _indexes[idx] = _indexes[idxtoswap];
                    _indexes[idxtoswap] = touched;
                }
            }
        }
    }
}