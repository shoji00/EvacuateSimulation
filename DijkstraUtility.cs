using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EvacuateSimulation_MultiAgent
{
    public static class DijkstraUtility
    {
        /// <summary>
        /// ノード間を中心線とする矩形の辺と全ての席との衝突判定をチェックする関数
        /// </summary>
        /// 
        /// <remarks>
        ///         |------------------|
        ///  ノード ○----------------○ノード
        ///         |------------------|
        /// </remarks>
        /// 
        /// <param name="node1">ノード1</param>
        /// <param name="node2">ノード2</param>
        /// <param name="tolerance">許容範囲</param>
        /// <returns>
        /// true:衝突していない
        /// false:衝突している
        /// </returns>
        public static bool DetermineCollision(Node node1, Node node2, double tolerance, TheaterLayoutParam theater)
        {
            var x1 = node1.X;
            var x2 = node2.X;
            var y1 = node1.Y;
            var y2 = node2.Y;

            double theta = Math.Atan2(y2 - y1, x2 - x1);

            //上辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x1 - tolerance * Math.Sin(theta), y1 + tolerance * Math.Cos(theta)),
                new Point(x2 - tolerance * Math.Sin(theta), y2 + tolerance * Math.Cos(theta)),
                theater))
            {
                return false;
            }

            //右辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x2 - tolerance * Math.Sin(theta), y2 + tolerance * Math.Cos(theta)),
                new Point(x2 + tolerance * Math.Sin(theta), y2 - tolerance * Math.Cos(theta)),
                theater))
            {
                return false;
            }

            //下辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x2 + tolerance * Math.Sin(theta), y2 - tolerance * Math.Cos(theta)),
                new Point(x1 + tolerance * Math.Sin(theta), y1 - tolerance * Math.Cos(theta)),
                theater))
            {
                return false;
            }

            //左辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x1 + tolerance * Math.Sin(theta), y1 - tolerance * Math.Cos(theta)),
                new Point(x1 - tolerance * Math.Sin(theta), y1 + tolerance * Math.Cos(theta)),
                theater))
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// 辺と席との衝突判定
        /// </summary>
        /// <param name="r1">辺の座標1</param>
        /// <param name="r2">辺の座標2</param>
        /// <returns>
        /// true:衝突している
        /// false:衝突していない
        /// </returns>
        private static bool CheckCollisionDeterminationWithAllSeatsAndSide(Point r1, Point r2, TheaterLayoutParam theater)
        {
            bool t1, t2;

            foreach (var seats in theater.Seats)
            {
                foreach (var seat in seats)
                {
                    var p1 = new Point(seat.PositionX - (seat.Width / 2), seat.PositionY - (seat.Height / 2));
                    var p2 = new Point(seat.PositionX + (seat.Width / 2), seat.PositionY - (seat.Height / 2));

                    //点を入れ替えて判定しないといけない
                    t1 = CheckCollisionSide(r1, r2, p1, p2);
                    t2 = CheckCollisionSide(p1, p2, r1, r2);

                    if (t1 && t2)
                    {
                        return true;
                    }

                    p1 = new Point(seat.PositionX + (seat.Width / 2), seat.PositionY - (seat.Height / 2));
                    p2 = new Point(seat.PositionX + (seat.Width / 2), seat.PositionY + (seat.Height / 2));

                    t1 = CheckCollisionSide(r1, r2, p1, p2);
                    t2 = CheckCollisionSide(p1, p2, r1, r2);

                    if (t1 && t2)
                    {
                        return true;
                    }

                    p1 = new Point(seat.PositionX + (seat.Width / 2), seat.PositionY + (seat.Height / 2));
                    p2 = new Point(seat.PositionX - (seat.Width / 2), seat.PositionY + (seat.Height / 2));

                    t1 = CheckCollisionSide(r1, r2, p1, p2);
                    t2 = CheckCollisionSide(p1, p2, r1, r2);

                    if (t1 && t2)
                    {
                        return true;
                    }

                    p1 = new Point(seat.PositionX - (seat.Width / 2), seat.PositionY + (seat.Height / 2));
                    p2 = new Point(seat.PositionX - (seat.Width / 2), seat.PositionY - (seat.Height / 2));

                    t1 = CheckCollisionSide(r1, r2, p1, p2);
                    t2 = CheckCollisionSide(p1, p2, r1, r2);

                    if (t1 && t2)
                    {
                        return true;
                    }
                }
            }

            return false; //クロスしていない
        }

        /// <summary>
        /// 辺と辺の衝突判定
        /// </summary>
        /// <param name="r1">辺1の座標1</param>
        /// <param name="r2">辺1の座標2</param>
        /// <param name="p1">辺2の座標1</param>
        /// <param name="p2">辺2の座標2</param>
        /// <returns>
        /// true:衝突している
        /// false:衝突していない
        /// </returns>
        private static bool CheckCollisionSide(Point r1, Point r2, Point p1, Point p2)
        {
            double t1, t2;

            //衝突判定計算
            t1 = (r1.X - r2.X) * (p1.Y - r1.Y) + (r1.Y - r2.Y) * (r1.X - p1.X);
            t2 = (r1.X - r2.X) * (p2.Y - r1.Y) + (r1.Y - r2.Y) * (r1.X - p2.X);

            //それぞれの正負が異なる（積が負になる）か、0（点が直線上にある）
            //ならクロスしている
            if (t1 * t2 < 0)
            {
                return (true); //クロスしている
            }
            else
            {
                return (false); //クロスしない
            }
        }
    }
}
