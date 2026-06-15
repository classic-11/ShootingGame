using ShootingGame.Model;
using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ShootingGame
{
    public partial class MainWindow : Window
    {
        GameModel Model = new GameModel();
        DispatcherTimer timer = new DispatcherTimer();
        const double SPEED = 4.0;
        const double ROT_SPEED = 0.05;
        bool keyLeft, keyRight, keySpace;
        int fireCounter = 0;
        int safetyFrames = 600;
        const int FIRE_RATE = 8;
        bool playExplosion = false;

        SoundPlayer shootPlayer = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "Sounds\\shoot.wav");
        SoundPlayer explosionPlayer = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "Sounds\\explosion.wav");

        public MainWindow()
        {
            InitializeComponent();
            shootPlayer.Load();
            explosionPlayer.Load();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) => GameLoop();
            Loaded += (s, e) =>
            {
                Model.Gun = new Gun(450, 480);
                SpawnShapes();
                timer.Start();
                GameCanvas.Focus();
            };
        }

        void SpawnShapes()
        {
            Random rnd = new Random();
            for (int i = 0; i < 3; i++)
            {
                Model.Circles.Add(new Circle(rnd.Next(100, 800), rnd.Next(30, 80), 30));
                Model.Rectangles.Add(new MyRectangle(rnd.Next(100, 800), rnd.Next(30, 80), 60, 40));
                Model.Triangles.Add(new MyTriangle(rnd.Next(100, 800), rnd.Next(30, 80), 35));
            }
        }

        void GameLoop()
        {
            if (!Model.Gun.IsAlive)
            {
                timer.Stop();
                MessageBox.Show("A shape hit the gun! GAME OVER!");
                return;
            }
            HandleInput();
            MoveShapes();
            ShapeCollisions();
            MoveBullets();
            TickExplosions();
            CheckGunHit();
            Render();
            if (playExplosion)
            {
                playExplosion = false;
                timer.Stop();
                explosionPlayer.Play();
                System.Threading.Thread.Sleep(800);
                timer.Start();
            }
        }

        void HandleInput()
        {
            if (keyLeft) Model.Gun.Angle -= ROT_SPEED;
            if (keyRight) Model.Gun.Angle += ROT_SPEED;
            if (keySpace)
            {
                fireCounter++;
                if (fireCounter >= FIRE_RATE) { Shoot(); fireCounter = 0; }
            }
        }

        void Shoot()
        {
            double tipX = Model.Gun.X + Math.Cos(Model.Gun.Angle) * 40;
            double tipY = Model.Gun.Y + Math.Sin(Model.Gun.Angle) * 40;
            Model.Bullets.Add(new Bullet(tipX, tipY, Model.Gun.Angle));
            shootPlayer.Play();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) keyLeft = true;
            if (e.Key == Key.Right) keyRight = true;
            if (e.Key == Key.Up)
            {
                keySpace = true;
                if (fireCounter == 0) { Shoot(); fireCounter = 1; }
            }
            if (e.Key == Key.Space)
            {
                keySpace = true;
                if (fireCounter == 0) { Shoot(); fireCounter = 1; }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) keyLeft = false;
            if (e.Key == Key.Right) keyRight = false;
            if (e.Key == Key.Up) keySpace = false;
            if (e.Key == Key.Space) { keySpace = false; fireCounter = 0; }
        }



        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GameCanvas.Focus();
        }

        private void TestSound_Click(object sender, RoutedEventArgs e)
        {
            shootPlayer.Play();
            MessageBox.Show("Did you hear the sound?");
        }

        void MoveShapes()
        {
            double W = GameCanvas.ActualWidth;
            double H = GameCanvas.ActualHeight;
            foreach (Circle c in Model.Circles)
            {
                c.X += c.XDirection * SPEED;
                c.Y += c.YDirection * SPEED;
                if (c.X - c.Radius <= 0 || c.X + c.Radius >= W) c.XDirection = -c.XDirection;
                if (c.Y - c.Radius <= 0 || c.Y + c.Radius >= H) c.YDirection = -c.YDirection;
            }
            foreach (MyRectangle r in Model.Rectangles)
            {
                r.X += r.XDirection * SPEED;
                r.Y += r.YDirection * SPEED;
                if (r.X - r.Width / 2 <= 0 || r.X + r.Width / 2 >= W) r.XDirection = -r.XDirection;
                if (r.Y - r.Height / 2 <= 0 || r.Y + r.Height / 2 >= H) r.YDirection = -r.YDirection;
            }
            foreach (MyTriangle t in Model.Triangles)
            {
                t.X += t.XDirection * SPEED;
                t.Y += t.YDirection * SPEED;
                if (t.X - t.Size <= 0 || t.X + t.Size >= W) t.XDirection = -t.XDirection;
                if (t.Y - t.Size <= 0 || t.Y + t.Size >= H) t.YDirection = -t.YDirection;
            }
        }

        void ShapeCollisions()
        {
            List<object> all = new List<object>();
            foreach (Circle c in Model.Circles) all.Add(c);
            foreach (MyRectangle r in Model.Rectangles) all.Add(r);
            foreach (MyTriangle t in Model.Triangles) all.Add(t);
            for (int i = 0; i < all.Count; i++)
            {
                for (int j = i + 1; j < all.Count; j++)
                {
                    GetCenter(all[i], out double ax, out double ay, out double ar);
                    GetCenter(all[j], out double bx, out double by, out double br);
                    double dx = bx - ax, dy = by - ay;
                    if (Math.Sqrt(dx * dx + dy * dy) < ar + br)
                    {
                        ReverseDir(all[i]);
                        ReverseDir(all[j]);
                    }
                }
            }
        }

        void GetCenter(object obj, out double x, out double y, out double r)
        {
            if (obj is Circle c) { x = c.X; y = c.Y; r = c.Radius; }
            else if (obj is MyRectangle rect) { x = rect.X; y = rect.Y; r = Math.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height) / 2; }
            else { MyTriangle t = (MyTriangle)obj; x = t.X; y = t.Y; r = t.Size; }
        }

        void ReverseDir(object obj)
        {
            if (obj is Circle c) { c.XDirection = -c.XDirection; c.YDirection = -c.YDirection; }
            else if (obj is MyRectangle r) { r.XDirection = -r.XDirection; r.YDirection = -r.YDirection; }
            else if (obj is MyTriangle t) { t.XDirection = -t.XDirection; t.YDirection = -t.YDirection; }
        }

        void MoveBullets()
        {
            double W = GameCanvas.ActualWidth;
            double H = GameCanvas.ActualHeight;
            List<Bullet> toRemove = new List<Bullet>();
            foreach (Bullet b in Model.Bullets)
            {
                b.X += Math.Cos(b.Angle) * b.Speed;
                b.Y += Math.Sin(b.Angle) * b.Speed;
                if (b.X < 0 || b.X > W || b.Y < 0 || b.Y > H) { toRemove.Add(b); continue; }
                foreach (Circle c in Model.Circles)
                {
                    double dx = b.X - c.X, dy = b.Y - c.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) < c.Radius)
                    {
                        c.HP--; toRemove.Add(b);
                        if (c.HP <= 0) { Model.Explosions.Add(new Explosion(c.X, c.Y)); playExplosion = true; }
                        break;
                    }
                }
                foreach (MyRectangle r in Model.Rectangles)
                {
                    double dx = b.X - r.X, dy = b.Y - r.Y;
                    double br = Math.Sqrt(r.Width * r.Width + r.Height * r.Height) / 2;
                    if (Math.Sqrt(dx * dx + dy * dy) < br)
                    {
                        r.HP--; toRemove.Add(b);
                        if (r.HP <= 0) { Model.Explosions.Add(new Explosion(r.X, r.Y)); playExplosion = true; }
                        break;
                    }
                }
                foreach (MyTriangle t in Model.Triangles)
                {
                    double dx = b.X - t.X, dy = b.Y - t.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) < t.Size)
                    {
                        t.HP--; toRemove.Add(b);
                        if (t.HP <= 0) { Model.Explosions.Add(new Explosion(t.X, t.Y)); playExplosion = true; }
                        break;
                    }
                }
            }
            foreach (Bullet b in toRemove) Model.Bullets.Remove(b);
            Model.Circles.RemoveAll(c => c.HP <= 0);
            Model.Rectangles.RemoveAll(r => r.HP <= 0);
            Model.Triangles.RemoveAll(t => t.HP <= 0);
        }

        void TickExplosions()
        {
            foreach (Explosion ex in Model.Explosions) { ex.Frame++; ex.Radius += 6; }
            Model.Explosions.RemoveAll(ex => ex.Frame >= ex.MaxFrames);
        }

        void CheckGunHit()
        {
            if (safetyFrames > 0) { safetyFrames--; return; }
            double gx = Model.Gun.X, gy = Model.Gun.Y;
            foreach (Circle c in Model.Circles)
            {
                double dx = c.X - gx, dy = c.Y - gy;
                if (Math.Sqrt(dx * dx + dy * dy) < c.Radius + 20) { Model.Gun.IsAlive = false; return; }
            }
            foreach (MyRectangle r in Model.Rectangles)
            {
                double dx = r.X - gx, dy = r.Y - gy;
                if (Math.Sqrt(dx * dx + dy * dy) < 50) { Model.Gun.IsAlive = false; return; }
            }
            foreach (MyTriangle t in Model.Triangles)
            {
                double dx = t.X - gx, dy = t.Y - gy;
                if (Math.Sqrt(dx * dx + dy * dy) < t.Size + 20) { Model.Gun.IsAlive = false; return; }
            }
        }

        void Render()
        {
            GameCanvas.Children.Clear();
            foreach (Circle c in Model.Circles) DrawCircle(c);
            foreach (MyRectangle r in Model.Rectangles) DrawRectangle(r);
            foreach (MyTriangle t in Model.Triangles) DrawTriangle(t);
            foreach (Bullet b in Model.Bullets) DrawBullet(b);
            foreach (Explosion ex in Model.Explosions) DrawExplosion(ex);
            DrawGun();
            int total = Model.Circles.Count + Model.Rectangles.Count + Model.Triangles.Count;
            if (total == 0 && Model.Explosions.Count == 0) { timer.Stop(); MessageBox.Show("YOU WIN!"); }
        }

        System.Windows.Media.Brush HpColor(int hp)
        {
            if (hp >= 5) return System.Windows.Media.Brushes.LimeGreen;
            if (hp == 4) return System.Windows.Media.Brushes.Yellow;
            if (hp == 3) return System.Windows.Media.Brushes.Orange;
            if (hp == 2) return System.Windows.Media.Brushes.Red;
            return System.Windows.Media.Brushes.DarkRed;
        }

        void DrawCircle(Circle c)
        {
            System.Windows.Shapes.Ellipse e = new System.Windows.Shapes.Ellipse();
            e.Width = c.Radius * 2; e.Height = c.Radius * 2;
            e.Stroke = HpColor(c.HP); e.StrokeThickness = 2;
            GameCanvas.Children.Add(e);
            Canvas.SetLeft(e, c.X - c.Radius); Canvas.SetTop(e, c.Y - c.Radius);
        }

        void DrawRectangle(MyRectangle r)
        {
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Width = r.Width; rect.Height = r.Height;
            rect.Stroke = HpColor(r.HP); rect.StrokeThickness = 2;
            GameCanvas.Children.Add(rect);
            Canvas.SetLeft(rect, r.X - r.Width / 2); Canvas.SetTop(rect, r.Y - r.Height / 2);
        }

        void DrawTriangle(MyTriangle t)
        {
            double a0 = -Math.PI / 2;
            double a1 = a0 + 2 * Math.PI / 3;
            double a2 = a1 + 2 * Math.PI / 3;
            System.Windows.Shapes.Polygon poly = new System.Windows.Shapes.Polygon();
            poly.Stroke = HpColor(t.HP); poly.StrokeThickness = 2;
            poly.Points = new System.Windows.Media.PointCollection
            {
                new System.Windows.Point(t.X + t.Size * Math.Cos(a0), t.Y + t.Size * Math.Sin(a0)),
                new System.Windows.Point(t.X + t.Size * Math.Cos(a1), t.Y + t.Size * Math.Sin(a1)),
                new System.Windows.Point(t.X + t.Size * Math.Cos(a2), t.Y + t.Size * Math.Sin(a2))
            };
            GameCanvas.Children.Add(poly);
        }

        void DrawBullet(Bullet b)
        {
            System.Windows.Shapes.Ellipse e = new System.Windows.Shapes.Ellipse();
            e.Width = 8; e.Height = 8; e.Fill = System.Windows.Media.Brushes.White;
            GameCanvas.Children.Add(e);
            Canvas.SetLeft(e, b.X - 4); Canvas.SetTop(e, b.Y - 4);
        }

        void DrawExplosion(Explosion ex)
        {
            System.Windows.Shapes.Ellipse e = new System.Windows.Shapes.Ellipse();
            e.Width = ex.Radius * 2; e.Height = ex.Radius * 2;
            e.Fill = System.Windows.Media.Brushes.OrangeRed;
            e.Opacity = 1.0 - (double)ex.Frame / ex.MaxFrames;
            GameCanvas.Children.Add(e);
            Canvas.SetLeft(e, ex.X - ex.Radius); Canvas.SetTop(e, ex.Y - ex.Radius);
        }

        void DrawGun()
        {
            double tipX = Model.Gun.X + Math.Cos(Model.Gun.Angle) * 40;
            double tipY = Model.Gun.Y + Math.Sin(Model.Gun.Angle) * 40;
            System.Windows.Shapes.Ellipse base_ = new System.Windows.Shapes.Ellipse();
            base_.Width = 30; base_.Height = 30;
            base_.Fill = System.Windows.Media.Brushes.Gray;
            GameCanvas.Children.Add(base_);
            Canvas.SetLeft(base_, Model.Gun.X - 15); Canvas.SetTop(base_, Model.Gun.Y - 15);
            System.Windows.Shapes.Line barrel = new System.Windows.Shapes.Line();
            barrel.X1 = Model.Gun.X; barrel.Y1 = Model.Gun.Y;
            barrel.X2 = tipX; barrel.Y2 = tipY;
            barrel.Stroke = System.Windows.Media.Brushes.White;
            barrel.StrokeThickness = 5;
            GameCanvas.Children.Add(barrel);
        }
    }
}