namespace ShootingGame.Model
{
    class MyTriangle
    {
        public double X;
        public double Y;
        public double Size;
        public int XDirection = 1;
        public int YDirection = 1;
        public int HP = 5;

        public MyTriangle(double x, double y, double size)
        {
            X = x;
            Y = y;
            Size = size;
        }
    }
}