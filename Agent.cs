using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvacuateSimulation_MultiAgent
{
    public class AgentBase
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// エージェントの半径
        /// </summary>
        public double Radius { get; set; } = 25.0;

        /// <summary>
        /// エージェントの速度
        /// </summary>
        public double Speed { get; set; } = 15.0;

        /// <summary>
        /// 距離のみの総コスト
        /// </summary>
        public double DistanceCost { get; set; } = 0.0;

        /// <summary>
        /// エージェントの現在地のノード
        /// </summary>
        public Node AgentNode { get; set; }

        /// <summary>
        /// 実際に避難する経路上のノード
        /// エージェントが向かう順に並べる
        /// </summary>
        public List<Node> RouteNode { get; set; } = new List<Node>();

        public AgentBase(double x, double y)
        {
            X = x;
            Y = y;
            AgentNode = new Node(X, Y, NodeKind.Start);
        }
    }
}
