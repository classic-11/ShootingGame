namespace ShootingGame.Model
{
    class MyRectangle
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public int XDirection = 1;
        public int YDirection = 1;
        public int HP = 5;

        public MyRectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}