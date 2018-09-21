using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvacuateSimulation_MultiAgent
{
    /// <summary>
    /// ノードクラス
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 現在のノードの状態
        /// </summary>
        public NodeKind NodeStatus { get; set; }

        /// <summary>
        /// 中心のX座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// 中心のY座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 現在のノードの1つ前のノード
        /// </summary>
        public Node PreviousNode { get; set; }

        /// <summary>
        /// 移動できるノード候補
        /// </summary>
        public List<Node> NextNodes { get; set; } = new List<Node>();

        /// <summary>
        /// ノードの半径
        /// </summary>
        public double Radius { get; set; } = 10;

        /// <summary>
        /// このノードまでの最小距離コスト
        /// </summary>
        public double DistanceCost { get; set; } = double.MaxValue;

        //-----------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">x座標</param>
        /// <param name="y">y座標</param>
        /// <param name="kind">ノードの種類</param>
        //-----------------------------------------------------------------------
        public Node(double x, double y, NodeKind kind = NodeKind.Unsearched)
        {
            X = x;
            Y = y;
            NodeStatus = kind;
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// 
        /// <remarks>
        /// 全てのプロパティをコピーする必要はないので要りそうなものだけ
        /// </remarks>
        /// 
        /// <param name="node">コピーしたいノード</param>
        //-----------------------------------------------------------------------
        public Node(Node node)
        {
            NodeStatus = node.NodeStatus;

            X = node.X;
            Y = node.Y;

            Radius = node.Radius;

            DistanceCost = node.DistanceCost;

            PreviousNode = node.PreviousNode?.Clone();
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// ダイクストラ法を用いた探索
        /// </summary>
        /// 
        /// <remarks>再帰関数</remarks>
        /// 
        /// <param name="nodes">移動候補先全てのノード</param>
        /// <param name="fixedNodes">確定ノードの保存先（参照先）</param>
        //-----------------------------------------------------------------------
        public void DoDijkstra(List<Node> nodes, ref Node fixedNode)
        {
            foreach(var node in nodes)
            {
                //もしも自身を含めてダイクストラ法を用いてしまうと無限ループになる可能性あり
                if (this == node)
                {
                    continue;
                }
                //確定ノードの場合はさらにその先まで探索する
                else if (node.NodeStatus == NodeKind.Determined)
                {
                    if (node.NextNodes.Count() == 0)
                    {
                        break;
                    }

                    //確定したルート通りでないと堂々巡りとなり探索が終わらない
                    if(node.PreviousNode != this)
                    {
                        continue;
                    }

                    //ダイクストラで探索
                    node.DoDijkstra(node.NextNodes, ref fixedNode);
                }
                //確定ノードじゃない時はコストを計算する
                else
                {
                    //コスト計算
                    // TODO 距離以外も用いた計算の実装
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
                        
                    }
                    //コストが今までの探索で得たそのノードのコストより小さかったら更新
                    else if (node.DistanceCost > temporaryCost)
                    {
                        node.DistanceCost = temporaryCost;
                        node.PreviousNode = this;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// コストを計算
        /// </summary>
        /// <param name="node">移動候補先のノード</param>
        /// <returns>コスト</returns>
        //-----------------------------------------------------------------------
        private double CalculateCost(Node node)
        {
            //100000に意味はない充分に大きい値であれば良し
            //コストが無限の時にこの関数を呼ぶことはないはずなのでこの条件式はいらない
            //が、念のため書いておく
            if(DistanceCost >= 100000)
            {
                return this.DistanceFromNode(node);
            }

            return DistanceCost + this.DistanceFromNode(node);
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// ディープコピー
        /// </summary>
        /// <returns>コピーしたノード</returns>
        //-----------------------------------------------------------------------
        public Node Clone()
        {
            //コピーコンストラクタ
            //クラスは参照型なので
            //return this;
            //をしてしまうとこのクラスの参照を渡してしまう
            return new Node(this);
        }
    }

    /// <summary>
    /// ノードの拡張クラス
    /// </summary>
    public static class NodeExpansion
    {
        //-----------------------------------------------------------------------
        /// <summary>
        /// ノード間の距離を計算する
        /// </summary>
        /// <param name="node1">自身のノード</param>
        /// <param name="node2">ノード</param>
        /// <returns></returns>
        //-----------------------------------------------------------------------
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
        Goal
    }
}
