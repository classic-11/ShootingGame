namespace ShootingGame.Model
{
    class Bullet
    {
        public double X;
        public double Y;
        public double Angle;
        public double Speed = 12.0;

        public Bullet(double x, double y, double angle)
        {
            X = x;
            Y = y;
            Angle = angle;
        }
    }
}