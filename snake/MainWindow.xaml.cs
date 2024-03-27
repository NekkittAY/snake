using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Shapes;
using System.IO;



namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        private const int CellSize = 20;
        private const int BoardWidth = 20;
        private const int BoardHeight = 20;
        private readonly List<Point> snake = new List<Point>();
        private Point food;
        private DispatcherTimer timer;
        private bool isMoving = false;
        private Direction direction = Direction.Right;

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            StartGame();
        }

        private void InitializeGame()
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                for (int y = 0; y < BoardHeight; y++)
                {
                    var rect = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = Brushes.LightGray
                    };
                    Canvas.SetLeft(rect, x * CellSize);
                    Canvas.SetTop(rect, y * CellSize);
                    GameBoard.Children.Add(rect);
                }
            }

            snake.Add(new Point(5, 5));

            PlaceFood();
        }

        private void StartGame()
        {
            timer = new DispatcherTimer();
            timer.Tick += Update;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
            isMoving = true;
        }

        private void Update(object sender, EventArgs e)
        {
            MoveSnake();

            if (CheckCollision())
            {
                EndGame();
            }
            else
            {
                if (snake[0] == food)
                {
                    snake.Add(snake.Last());
                    PlaceFood();
                }
            }
        }

        private void MoveSnake()
        {
            for (int i = snake.Count - 1; i >= 1; i--)
            {
                snake[i] = snake[i - 1];
            }

            var head = snake.First();
            switch (direction)
            {
                case Direction.Up:
                    snake[0] = new Point(head.X, head.Y - 1);
                    break;
                case Direction.Down:
                    snake[0] = new Point(head.X, head.Y + 1);
                    break;
                case Direction.Left:
                    snake[0] = new Point(head.X - 1, head.Y);
                    break;
                case Direction.Right:
                    snake[0] = new Point(head.X + 1, head.Y);
                    break;
            }

            DrawSnake();
        }

        private void DrawSnake()
        {
            GameBoard.Children.OfType<Rectangle>().ToList().ForEach(x =>
            {
                if (x.Fill != Brushes.Red)
                {
                    x.Fill = Brushes.LightGray;
                }
            });

            foreach (var segment in snake)
            {
                foreach (Rectangle rect in GameBoard.Children.OfType<Rectangle>())
                {
                    int x = (int)Canvas.GetLeft(rect) / CellSize;
                    int y = (int)Canvas.GetTop(rect) / CellSize;

                    if (x == (int)segment.X && y == (int)segment.Y)
                    {
                        if (rect.Fill != Brushes.Red)
                        {
                            rect.Fill = Brushes.Black;
                        }
                    }
                }
            }
        }

        private Rectangle foodRect = new Rectangle
        {
            Width = CellSize,
            Height = CellSize,
            Fill = Brushes.Red
        };

        private void PlaceFood()
        {
            var rand = new Random();
            int x, y;
            do
            {
                x = rand.Next(BoardWidth);
                y = rand.Next(BoardHeight);
            } while (snake.Contains(new Point(x, y)));

            food = new Point(x, y);

            Canvas.SetLeft(foodRect, x * CellSize);
            Canvas.SetTop(foodRect, y * CellSize);

            if (!GameBoard.Children.Contains(foodRect))
            {
                GameBoard.Children.Add(foodRect);
            }
        }

        private bool CheckCollision()
        {
            var head = snake.First();
            return head.X < 0 || head.X >= BoardWidth || head.Y < 0 || head.Y >= BoardHeight ||
                   snake.Skip(1).Any(segment => segment == head);
        }

        private int initialSnakeLength;

        private void SaveResultToFile()
        {
            string result = $"Player: {Environment.UserName}, Score: {snake.Count - initialSnakeLength}";
            string path = "results.txt";

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(result);
            }
        }

        private void EndGame()
        {
            timer.Stop();
            isMoving = false;

            SaveResultToFile();

            MessageBox.Show($"Game Over! Your score: {snake.Count}. Your result has been saved.", "Game Over", MessageBoxButton.OK);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isMoving) return;

            switch (e.Key)
            {
                case Key.Up:
                    if (direction != Direction.Down)
                        direction = Direction.Up;
                    break;
                case Key.Down:
                    if (direction != Direction.Up)
                        direction = Direction.Down;
                    break;
                case Key.Left:
                    if (direction != Direction.Right)
                        direction = Direction.Left;
                    break;
                case Key.Right:
                    if (direction != Direction.Left)
                        direction = Direction.Right;
                    break;
            }
        }
    }
}
