using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvacuateSimulation_MultiAgent
{
    public class Node
    {
        public bool IsGoalNode { get; set; } 

        /// <summary>
        /// ノードの中心のX座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// ノードの中心のY座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 移動候補先ノード
        /// </summary>
        public List<Node> NextNodes { get; set; } = new List<Node>();

        /// <summary>
        /// ノードの半径
        /// </summary>
        public double Radius { get; set; } = 10;

        public Node(double x, double y, bool goalNode = false)
        {
            X = x;
            Y = y;
            IsGoalNode = goalNode;
        }
    }

    public static class NodeExpansion
    {
        public static double DistanceFromNode(this Node node1, Node node2)
        {
            return Math.Sqrt(
                (node1.X - node2.X) * (node1.X - node2.X)
                + (node1.Y - node2.Y) * (node1.Y - node2.Y));
        }
    }
}
