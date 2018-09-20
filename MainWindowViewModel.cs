using Newtonsoft.Json;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvacuateSimulation_MultiAgent
{
    public class MainWindowViewModel
    {
        /// <summary>
        /// 映画館のレイアウト
        /// </summary>
        private TheaterLayoutParam theater_;

        /// <summary>
        /// ノードのリスト
        /// </summary>
        private List<Node> nodes_;

        /// <summary>
        /// XamlにあるImageコントロール
        /// Todo:これがなくてもサイズを変更できるようにする
        /// </summary>
        private Image image_;

        /// <summary>
        /// エージェントのリスト
        /// </summary>
        private List<AgentBase> agents_;

        /// <summary>
        /// 表示する画像のビットマップ
        /// </summary>
        public RenderTargetBitmap Bitmap { get; set; }

        /// <summary>
        /// レンダリング用のビジュアル
        /// </summary>
        public DrawingVisual DrawVisual { get; set; }

        /// <summary>
        /// ビジュアルコンテンツ
        /// </summary>
        public DrawingContext DrawContext { get; set; }

        /// <summary>
        /// レイアウト情報を読み込むコマンド
        /// </summary>
        public ReactiveCommand LoadLayoutCommand { get; set; }

        /// <summary>
        /// Xamlデザイナー用のコンストラクタ
        /// </summary>
        public MainWindowViewModel() 
            : this(new Image())
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="image">XamlにあるImageコントロール</param>
        public MainWindowViewModel(Image image)
        {
            theater_ = new TheaterLayoutParam();

            nodes_ = new List<Node>();

            image_ = image;

            agents_ = new List<AgentBase>();

            Bitmap = new RenderTargetBitmap(
                1000,
                1000,
                96,
                96,
                PixelFormats.Default);

            DrawVisual = new DrawingVisual();

            LoadLayoutCommand = new ReactiveCommand();

            LoadLayoutCommand.Subscribe(_ => OpenFile());
        }

        /// <summary>
        /// Jsonファイルからレイアウト情報を読み込む関数
        /// </summary>
        public void OpenFile()
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Json Files(.json)|*.json";
                fileDialog.Title = "開くファイルを選択してください";
                fileDialog.Multiselect = false;


                //ダイアログを表示する
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var streamReader = new StreamReader(fileDialog.FileName))
                    {
                        var json = streamReader.ReadToEnd();

                        theater_ = JsonConvert.DeserializeObject<TheaterLayoutParam>(json);

                        nodes_.Clear();

                        agents_.Clear();

                        //ノードの設定
                        foreach (var seats in theater_.Seats)
                        {
                            double distance = 25;

                            IntegrateNodes(new Node(
                                seats[0].PositionX - (seats[0].Width / 2) - distance,
                                seats[0].PositionY - (seats[0].Height / 2) - distance));
                            IntegrateNodes(new Node(
                                seats[0].PositionX - (seats[0].Width / 2) - distance,
                                seats[0].PositionY + (seats[0].Height / 2) + distance));
                            IntegrateNodes(new Node(
                                seats[seats.Count() - 1].PositionX + (seats[0].Width / 2) + distance,
                                seats[seats.Count() - 1].PositionY - (seats[0].Height / 2) - distance));
                            IntegrateNodes(new Node(
                                seats[seats.Count() - 1].PositionX + (seats[0].Width / 2) + distance,
                                seats[seats.Count() - 1].PositionY + (seats[0].Height / 2) + distance));
                        }

                        //出口の設定
                        foreach (var goal in theater_.Goals)
                        {
                            nodes_.Add(new Node(goal.PositionX, goal.PositionY, NodeKind.Goal));
                        }

                        //経路の設定

                        foreach (var node1 in nodes_)
                        {
                            if (node1.NodeStatus == NodeKind.Goal)
                            {
                                continue;
                            }

                            foreach (var node2 in nodes_)
                            {
                                if (node1 == node2)
                                {
                                    continue;
                                }

                                if (DijkstraUtility.DetermineCollision(node1, node2, 25, theater_))
                                {
                                    node1.NextNodesWithThickPath.Add(node2);
                                }/*
                                else if (CheckAllSidesCollision(node1, node2, 15))
                                {
                                    node1.NextNodesWithThinPath.Add(node2);
                                }
                                */
                            }
                        }


                        //エージェントの設定
                        agents_.Add(new AgentBase(300, 150));

                        //全てのエージェントで都度移動可能なノードを調べて探索する
                        foreach (var agent in agents_)
                        {
                            foreach (var node in nodes_)
                            {
                                if (DijkstraUtility.DetermineCollision(agent.AgentNode, node, 25, theater_))
                                {
                                    agent.AgentNode.NextNodes.Add(node);
                                }
                            }
                            
                            Node goalNode = null;
                            int num = 1;
                            File.WriteAllText(
                                    "D:\\VisualStudio\\Log.txt",
                                    $"--------------------------Start Dijkstra------------------------------" + Environment.NewLine + $"Agent: {agent.X}, {agent.Y}" + Environment.NewLine);

                            while (true)
                            {
                                File.AppendAllText(
                                    "D:\\VisualStudio\\Log.txt",
                                    $"-------{num}--------" + Environment.NewLine);

                                num++;

                                Node determinedNode = null;

                                agent.AgentNode.DoDijkstra(agent.AgentNode.NextNodes, ref determinedNode);

                                if (determinedNode == null)
                                {
                                    throw new Exception("ゴールにたどり着けません");
                                }

                                File.AppendAllText(
                                    "D:\\VisualStudio\\Log.txt",
                                    $"{determinedNode.X}, {determinedNode.Y}, cost = {determinedNode.DistanceCost}" + Environment.NewLine);

                                    if (determinedNode.NodeStatus == NodeKind.Goal)
                                    {
                                        goalNode = determinedNode;
                                        break;
                                    }

                                determinedNode.NodeStatus = NodeKind.Determined;

                                foreach (var node in nodes_)
                                {
                                    if (determinedNode == node
                                        || node.NodeStatus == NodeKind.Determined
                                        || node.NodeStatus == NodeKind.Fucking)
                                    {
                                        continue;
                                    }

                                    if (DijkstraUtility.DetermineCollision(determinedNode, node, 25, theater_))
                                    {
                                        determinedNode.NextNodes.Add(node);
                                    }
                                }
                            }

                            while (true)
                            {
                                agent.RouteNode.Add(goalNode);

                                goalNode = goalNode.PreviousNode;

                                if (goalNode.NodeStatus == NodeKind.Start)
                                {
                                    agent.RouteNode.Reverse();
                                    break;
                                }
                            }
                           
                            /*
                            foreach (var node in nodes_)
                            {
                                node.NextNode = null;
                                node.NextNodes.Clear();
                                node.NodeStatus = NodeKind.Unsearched;
                                node.PreviousNode = null;
                                node.DistanceCost = 0;
                            }
                            */
                        }
                        /*
                        foreach(var seats in theater_.Seats)
                        {
                            foreach(var seat in seats)
                            {
                                agents_.Add(new AgentBase(seat.PositionX, seat.PositionY - (seat.Height / 2) - 25));
                            }
                        }
                        */

                        DrawLayout();
                    }
                }
            }

        }

        /// <summary>
        /// ノードが他のノードの近くにあった時ノードを統合する
        /// </summary>
        /// <param name="node">ノード</param>
        public void IntegrateNodes(Node node)
        {
            foreach (var nodes in nodes_)
            {
                if (nodes.DistanceFromNode(node) < 25)
                {
                    nodes.X = (nodes.X + node.X) / 2.0;
                    nodes.Y = (nodes.Y + node.Y) / 2.0;
                    return;
                }
            }

            nodes_.Add(node);
        }
        
        /// <summary>
        /// レイアウトを描画する関数
        /// </summary>
        /// <param name="layout">レイアウトの情報</param>
        public void DrawLayout()
        {
            double radius = 5;

            Bitmap = new RenderTargetBitmap(
                theater_.Width,
                theater_.Height,
                96,
                96,
                PixelFormats.Default);

            //これをしないと画像が更新されない
            //正確に書けば画像のサイズ変更ができない
            image_.Source = Bitmap;

            DrawContext = DrawVisual.RenderOpen();

            //描画するオブジェクトの作成
            DrawContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, theater_.Width, theater_.Height));

            //座席の描画
            foreach (var seats in theater_.Seats)
            {
                foreach (var seat in seats)
                {
                    DrawContext.DrawRectangle(
                        Brushes.White,
                        new Pen(Brushes.Black, 1),
                        new Rect(seat.PositionX - seat.Width / 2, seat.PositionY - seat.Height / 2, seat.Width, seat.Height));
                }
            }

            //出口の描画
            foreach (var goal in theater_.Goals)
            {
                DrawContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(
                        goal.PositionX - goal.Width / 2,
                        goal.PositionY - goal.Height / 2,
                        goal.Width,
                        goal.Height));
            }

            //ノードの描画
            foreach (var node in nodes_)
            {
                DrawContext.DrawEllipse(
                    Brushes.Blue,
                    null,
                    new Point(node.X, node.Y),
                    radius,
                    radius);
            }

            //経路の描画
            foreach (var node1 in nodes_)
            {
                foreach (var node2 in node1.NextNodesWithThinPath)
                {
                    DrawContext.DrawLine(
                        new Pen(Brushes.Blue, 1),
                        new Point(node1.X, node1.Y),
                        new Point(node2.X, node2.Y));
                }

                foreach (var node2 in node1.NextNodesWithThickPath)
                {
                    DrawContext.DrawLine(
                        new Pen(Brushes.Black, 1),
                        new Point(node1.X, node1.Y),
                        new Point(node2.X, node2.Y));
                }
            }

            //エージェントの描画
            foreach (var agent in agents_)
            {
                DrawContext.DrawEllipse(
                    null,
                    new Pen(Brushes.Blue, 1),
                    new Point(agent.X, agent.Y),
                    agent.Radius,
                    agent.Radius);
                
                foreach(var node in agent.RouteNode)
                {
                    DrawContext.DrawLine(
                        new Pen(Brushes.Red, 10),
                        new Point(node.PreviousNode.X, node.PreviousNode.Y),
                        new Point(node.X, node.Y));
                }

                foreach(var node in agent.AgentNode.NextNodes)
                {
                     DrawContext.DrawLine(
                        new Pen(Brushes.Black, 1),
                        new Point(agent.AgentNode.X, agent.AgentNode.Y),
                        new Point(node.X, node.Y));
                }
                
            }

            DrawContext.Close();

            //表示する画像を更新 
            Bitmap.Render(DrawVisual);
        }
    }
}
