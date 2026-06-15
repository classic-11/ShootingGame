namespace ShootingGame.Model
{
    class Explosion
    {
        public double X;
        public double Y;
        public double Radius = 8;
        public int Frame = 0;
        public int MaxFrames = 18;

        public Explosion(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}