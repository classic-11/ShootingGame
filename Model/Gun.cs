using System;

namespace ShootingGame.Model
{
    class Gun
    {
        public double X;
        public double Y;
        public double Angle = -Math.PI / 2;  // points straight UP
        public bool IsAlive = true;

        public Gun(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}