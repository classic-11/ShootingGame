using System.Collections.Generic;

namespace ShootingGame.Model
{
    class GameModel
    {
        public List<Circle> Circles = new List<Circle>();
        public List<MyRectangle> Rectangles = new List<MyRectangle>();
        public List<MyTriangle> Triangles = new List<MyTriangle>();
        public List<Bullet> Bullets = new List<Bullet>();
        public List<Explosion> Explosions = new List<Explosion>();
        public Gun Gun = new Gun(0, 0);
    }
}