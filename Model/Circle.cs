namespace ShootingGame.Model
{
    class Circle
    {
        public double X;
        public double Y;
        public double Radius;
        public int XDirection = 1;
        public int YDirection = 1;
        public int HP = 5;

        public Circle(double x, double y, double radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }
    }
}