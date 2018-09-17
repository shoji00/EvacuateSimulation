using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvacuateSimulation_MultiAgent
{
    /// <summary>
    /// 座席の情報クラス
    /// </summary>
    public class TheaterSeatParam
    {
        /// <summary>
        /// 座席の横幅
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 座席の縦幅
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 座席のX座標
        /// </summary>
        public double PositionX { get; set; }

        /// <summary>
        /// 座席のY座標
        /// </summary>
        public double PositionY { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="width">座席の縦幅</param>
        /// <param name="height">座席の横幅</param>
        /// <param name="positionX">座席のX座標</param>
        /// <param name="positionY">座席のY座標</param>
        public TheaterSeatParam(double width, double height, double positionX, double positionY)
        {
            Height = height;
            Width = width;
            PositionX = positionX;
            PositionY = positionY;
        }
    }

    /// <summary>
    /// 出口の情報をもつクラス
    /// </summary>
    public class TheaterGoalParam
    {
        /// <summary>
        /// 出口の横幅
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 出口の縦幅
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 出口のX座標
        /// </summary>
        public double PositionX { get; set; }

        /// <summary>
        /// 出口のY座標
        /// </summary>
        public double PositionY { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="width">出口の縦幅</param>
        /// <param name="height">出口の横幅</param>
        /// <param name="positionX">出口のX座標</param>
        /// <param name="positionY">出口のY座標</param>
        public TheaterGoalParam(double width, double height, double positionX, double positionY)
        {
            Height = height;
            Width = width;
            PositionX = positionX;
            PositionY = positionY;
        }
    }

    /// <summary>
    /// 映画館のレイアウトクラス
    /// </summary>
    public class TheaterLayoutParam
    {

        /// <summary>
        /// レイアウトの横幅
        /// </summary>
        public int Width { get; set; } = 1000;

        /// <summary>
        /// レイアウトの縦幅
        /// </summary>
        public int Height { get; set; } = 1000;


        /// <summary>
        /// 座席のリスト（隣接している座席を1つのものとしてListに加えていく）
        /// </summary>
        public List<List<TheaterSeatParam>> Seats { get; set; } = new List<List<TheaterSeatParam>>();

        /// <summary>
        /// 出口のリスト
        /// </summary>
        public List<TheaterGoalParam> Goals { get; set; } = new List<TheaterGoalParam>();
    }
}
