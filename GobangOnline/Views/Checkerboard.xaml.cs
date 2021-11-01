using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GobangOnline.Models;
using GobangOnline.ViewModels;
using Microsoft.VisualBasic.CompilerServices;

namespace GobangOnline.Views
{
    /// <summary>
    /// CheckerBorder.xaml 的交互逻辑
    /// </summary>
    public partial class Checkerboard : UserControl
    {
        private IntPoint startupPoint = new IntPoint() { X = 30, Y = 30 };

        public PieceType CurrPieceType
        {
            get => chessChanges % 2 == 0 ? PieceType.White : PieceType.Black;
            set
            {
                _currPieceType = value;
                chessChanges = value == PieceType.Black ? 1 : 0;
            }
        }

        public struct IntPoint
        {
            public int X, Y;
        }
        public IntPoint[,] BoardCoordinateMatrix;

        public IntPoint ShortestDistancePoint(IntPoint p)
        {
            double minDistance = double.MaxValue;
            IntPoint minPoint = new IntPoint();
            if (p.X < startupPoint.X) p.X = startupPoint.X;
            if (p.Y < startupPoint.Y) p.Y = startupPoint.Y;
            foreach (var point in BoardCoordinateMatrix)
            {
                double distance = Math.Pow(p.X - point.X, 2) + Math.Pow(p.Y - point.Y, 2);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    minPoint = point;
                }
            }
            return minPoint;
        } 

        private void RefreshCoordinateMatrix(int aveWidth, int aveHeight)
        {
            var sumY = 0;
            for (int i = 0; i < CheckerboardLength; i++)
            {
                var sumX = 0;
                for (int j = 0; j < CheckerboardLength; j++)
                {
                    BoardCoordinateMatrix[i, j].X = sumX;
                    BoardCoordinateMatrix[i, j].Y = sumY;
                    sumX += aveWidth;
                }
                sumY += aveHeight;
            }
        }
        private static readonly int CheckerboardLength = GobangViewModel.CheckerboardLength;
        private GobangViewModel vm;
        private int aveWidth;
        private int aveHeight;
        private PieceType _currPieceType;
        private GameWay _gameWay = GameWay.Local;
        public Action<IntPoint> NetAddPiece { get; set; }

        public Checkerboard()
        {
            InitializeComponent();
            BoardCoordinateMatrix = new IntPoint[GobangViewModel.CheckerboardLength, GobangViewModel.CheckerboardLength];
            Board.Loaded += (sender, args) =>
            {
                vm = new GobangViewModel(piece =>
                {
                    //NetAddPiece?.Invoke(piece);
                    AddNewPiece(new IntPoint() { X = piece.X * aveWidth, Y = piece.Y * aveHeight }, piece.PieceType);
                });
                DataContext = vm;
                aveWidth = (int)Board.ActualWidth / CheckerboardLength;
                aveHeight = (int)Board.ActualHeight / CheckerboardLength;
                RefreshCoordinateMatrix(aveWidth, aveHeight);
                Draw(MainCanvas, startupPoint);
                vm.VictoryEvent += type =>
                {
                    //ClearCheckerBoard();
                    vm.Enabled = false;
                    vm.GameFinished = true;
                };
            };
            chessChanges = 1;
        }

        private IntPoint Index2Coordinate(IntPoint index)
        {
            return new IntPoint() { X = index.X * aveWidth, Y = index.Y * aveHeight };
        }

        private IntPoint Coordinate2Index(IntPoint p)
        {
            return new IntPoint() { X = p.X / aveWidth, Y = p.Y / aveHeight };
        }

        private void AddNewPiece(IntPoint point, PieceType type)
        {
            var Uid = (point.X, point.Y).ToString();
            if (type == PieceType.None)
            {
                UIElement? removeElement = null;
                foreach (UIElement pieceCanvasChild in PieceCanvas.Children)
                {
                    if (pieceCanvasChild.Uid == Uid)
                    {
                        removeElement = pieceCanvasChild;
                    }
                }
                if (removeElement != null)
                {
                    PieceCanvas.Children.Remove(removeElement);
                }
                return;
            }
            int r = 15;
            App.Current.Dispatcher.Invoke(() =>
            {
                Ellipse ellipse = new Ellipse() { Uid = Uid };
                ellipse.StrokeThickness = 10;
                ellipse.Fill = type == PieceType.Black ? Brushes.Black : Brushes.White;
                ellipse.Width = r;
                ellipse.Height = r;
                Canvas.SetTop(ellipse, point.Y - r / 2);
                Canvas.SetLeft(ellipse, point.X - r / 2);
                PieceCanvas.Children.Add(ellipse);
            });
        }

        public int chessChanges { get; set; }

        public GameWay gameWay
        {
            get => _gameWay;
            set => _gameWay = value;
        }

        public enum GameWay
        {
            Local,
            Net
        }

        public delegate void NetAddNewPieceEventHandler(IntPoint p);
        public event NetAddNewPieceEventHandler NetAddNewPiece;

        private void Board_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(Board);
            var minPoint = ShortestDistancePoint(new IntPoint(){X = (int)position.X, Y = (int)position.Y});
            //Textblock.Text = $"Y: {minPoint.X}, Y: {minPoint.Y}";
            //AddNewPiece(minPoint, PieceType.White);
            var index = Coordinate2Index(minPoint);
            var selectedPosi = vm.CheckerboardData[index.X, index.Y];
            if (selectedPosi.PieceType == PieceType.None)
            {
                if (gameWay == GameWay.Local)
                {
                    selectedPosi.PieceType = chessChanges % 2 == 0 ? PieceType.White : PieceType.Black;
                    chessChanges++;
                }
                else
                {
                    NetAddNewPiece?.Invoke(index);
                }
            }
        }

        public void ClearCheckerBoard()
        {
            PieceCanvas.Children.Clear();
            vm.ResetGame();
        }


        /// <summary>
        /// 画网格
        /// </summary>
        /// <param name="canvas">画板</param>
        /// <param name="StartupPoint">初始位置</param>
        public static void Draw(Canvas canvas, IntPoint? StartupPoint = null)
        {
            StartupPoint ??= new IntPoint() { X = 0, Y = 0 };
            var gridBrush = new SolidColorBrush { Color = Colors.Black };
            double scaleX = canvas.ActualWidth / CheckerboardLength;
            double scaleY = canvas.ActualHeight / CheckerboardLength;
            double currentPosY = StartupPoint.Value.Y;
            currentPosY += scaleX;

            while (currentPosY < canvas.ActualHeight + scaleY)
            {
                Line line = new Line
                {
                    X1 = StartupPoint.Value.X,
                    Y1 = currentPosY - scaleY,
                    X2 = canvas.ActualWidth - scaleY,
                    Y2 = currentPosY - scaleY,
                    Stroke = gridBrush,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
                currentPosY += scaleX;
            }

            double currentPosX = StartupPoint.Value.X;
            currentPosX += scaleY;
            while (currentPosX < canvas.ActualWidth + scaleX)
            {
                Line line = new Line
                {
                    X1 = currentPosX - scaleX,
                    Y1 = StartupPoint.Value.Y,
                    X2 = currentPosX - scaleX,
                    Y2 = canvas.ActualHeight - canvas.ActualHeight / 15,
                    Stroke = gridBrush,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
                currentPosX += scaleY;
            }
        }

    }
}
