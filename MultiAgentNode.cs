using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvacuateSimulation_MultiAgent
{
    public class Node
    {
        /// <summary>
        /// 現在のノードの状態
        /// </summary>
        public NodeKind NodeStatus { get; set; }

        /// <summary>
        /// ノードの中心のX座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// ノードの中心のY座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 現在のノードの1つ前のノード
        /// </summary>
        public Node PreviousNode { get; set; }

        /// <summary>
        /// 移動（候補）先のノード
        /// </summary>
        public Node NextNode { get; set; }

        /// <summary>
        /// 移動できるノード候補
        /// </summary>
        public List<Node> NextNodes { get; set; } = new List<Node>();

        /// <summary>
        /// 移動候補先ノード(太い経路)
        /// </summary>
        public List<Node> NextNodesWithThickPath { get; set; } = new List<Node>();

        /// <summary>
        /// 移動候補先ノード(細い経路)
        /// </summary>
        public List<Node> NextNodesWithThinPath { get; set; } = new List<Node>();

        /// <summary>
        /// ノードの半径
        /// </summary>
        public double Radius { get; set; } = 10;

        /// <summary>
        /// このノードまでの最小距離コスト
        /// </summary>
        public double DistanceCost { get; set; } = double.MaxValue;

        public Node(double x, double y, NodeKind kind = NodeKind.Unsearched)
        {
            X = x;
            Y = y;
            NodeStatus = kind;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="node">コピーしたいノード</param>
        public Node(Node node)
        {
            NodeStatus = node.NodeStatus;

            X = node.X;
            Y = node.Y;

            Radius = node.Radius;

            DistanceCost = node.DistanceCost;
        }

        /// <summary>
        /// ダイクストラ法を用いた探索
        /// </summary>
        /// <param name="nodes">移動候補先全てのノード</param>
        /// <returns>
        /// true:ゴールが見つかった
        /// false:ゴールが見つからなかった
        /// </returns>
        public void DoDijkstra(List<Node> nodes, ref Node fixedNode, int i = 0)
        {
            foreach(var node in nodes)
            {
                if (this == node)
                {
                    continue;
                }
                else if (node.NodeStatus == NodeKind.Determined)
                {
                    if (node.NextNodes.Count() == 0)
                    {
                        break;
                    }

                    if(node.PreviousNode != this)
                    {
                        continue;
                    }
                    File.AppendAllText(
                                    "D:\\VisualStudio\\Log.txt",
                                    $"determined:{node.X}, {node.Y}, {node.DistanceCost}" + Environment.NewLine);
                    node.DoDijkstra(node.NextNodes, ref fixedNode, ++i);
                }
                else
                {
                    var temporaryCost = CalculateCost(node);

                    if (fixedNode == null)
                    {
                        fixedNode = node;
                        node.DistanceCost = temporaryCost;
                        node.PreviousNode = this;                        
                    }
                    else if (fixedNode.DistanceCost > temporaryCost)
                    {
                        fixedNode = node;

                        if (node.DistanceCost > temporaryCost)
                        {
                            node.DistanceCost = temporaryCost;
                            node.PreviousNode = this;
                        }
                    }
                    else if (node.DistanceCost > temporaryCost)
                    {
                        node.DistanceCost = temporaryCost;
                        node.PreviousNode = this;
                    }
                }
            }
            
            File.AppendAllText(
                                "D:\\VisualStudio\\Log.txt",
                                $"{i}層目, {fixedNode.X}, {fixedNode.Y}, {fixedNode.DistanceCost}" + Environment.NewLine);
        }

        /// <summary>
        /// コストを計算
        /// </summary>
        /// <param name="node">移動候補先のノード</param>
        /// <returns>コスト</returns>
        private double CalculateCost(Node node)
        {
            if(DistanceCost >= 100000)
            {
                return this.DistanceFromNode(node);
            }

            return DistanceCost + this.DistanceFromNode(node);
        }

        /// <summary>
        /// ディープコピー
        /// </summary>
        /// <returns>コピーしたノード</returns>
        public Node Clone()
        {
            return new Node(this);
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

    /// <summary>
    /// ノードの種類
    /// </summary>
    public enum NodeKind
    {
        /// <summary>
        /// 未探索
        /// </summary>
        Unsearched,
        
        /// <summary>
        /// 探索済み
        /// </summary>
        Searched,
        
        /// <summary>
        /// 確定
        /// </summary>
        Determined,

        /// <summary>
        /// スタート
        /// </summary>
        Start,

        /// <summary>
        /// ゴール
        /// </summary>
        Goal,

        Fucking
    }
}
