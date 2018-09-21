using Newtonsoft.Json;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvacuateSimulation_MultiAgent
{
    /// <summary>
    /// MainWindowのViewModel
    /// </summary>
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

        //-----------------------------------------------------------------------
        /// <summary>
        /// Xamlデザイナー用のコンストラクタ
        /// </summary>
        //-----------------------------------------------------------------------
        public MainWindowViewModel() 
            : this(new Image())
        {
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="image">XamlにあるImageコントロール</param>
        //-----------------------------------------------------------------------
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

        //-----------------------------------------------------------------------
        /// <summary>
        /// Jsonファイルからレイアウト情報を読み込む関数
        /// </summary>
        //-----------------------------------------------------------------------
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

                        //エージェントの設定
                        agents_.Add(new AgentBase(300, 150));

                        //全てのエージェントで都度移動可能なノードを調べて探索する
                        foreach (var agent in agents_)
                        {
                            //最初にエージェントの現在地から移動可能なノードの候補を探す
                            foreach (var node in nodes_)
                            {
                                if (DijkstraUtility.IsColidedSomething(agent.Node, node, 25, theater_))
                                {
                                    agent.Node.NextNodes.Add(node);
                                }
                            }
                            
                            Node goalNode = null;
                            
                            while (true)
                            {
                                Node determinedNode = null;

                                //ダイクストラで探索
                                agent.Node.DoDijkstra(agent.Node.NextNodes, ref determinedNode);

                                //ゴールにたどり着けないエージェントがいるとき
                                //エージェントの初期位置がしっかりしていれば出ないはず
                                if (determinedNode == null)
                                {
                                    throw new Exception("ゴールにたどり着けません");
                                }

                                //ゴールなら終了
                                if (determinedNode.NodeStatus == NodeKind.Goal)
                                {
                                    goalNode = determinedNode.Clone();
                                    break;
                                }

                                //ノードを確定ノードに変更
                                determinedNode.NodeStatus = NodeKind.Determined;

                                //確定ノードから移動可能なノードを全てNextNodesに追加
                                foreach (var node in nodes_)
                                {
                                    if (determinedNode == node
                                        || node.NodeStatus == NodeKind.Determined)
                                    {
                                        continue;
                                    }

                                    if (DijkstraUtility.IsColidedSomething(determinedNode, node, 25, theater_))
                                    {
                                        determinedNode.NextNodes.Add(node);
                                    }
                                }
                            }

                            //出口までの経路をエージェントに保持させる
                            //この後レイアウト内のノードは全て初期化されるのでディープコピーしたものを渡す
                            //ゴールを見つけた時にゴールノードとゴールまでに通るノードをディープコピーしてあるためここでCloneする必要はない
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
                           
                            //ノードの初期化
                            foreach (var node in nodes_)
                            {
                                node.NextNodes.Clear();

                                if (node.NodeStatus != NodeKind.Goal)
                                {
                                    node.NodeStatus = NodeKind.Unsearched;
                                }

                                node.PreviousNode = null;
                                node.DistanceCost = double.MaxValue;
                            }
                        }
                        
                        DrawLayout();
                    }
                }
            }

        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// ノードが他のノードの近くにあった時ノードを統合する
        /// </summary>
        /// <param name="node">ノード</param>
        //-----------------------------------------------------------------------
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
        //-----------------------------------------------------------------------
        /// <summary>
        /// レイアウトの描画
        /// </summary>
        //-----------------------------------------------------------------------
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

            //エージェントの描画
            foreach (var agent in agents_)
            {
                DrawContext.DrawEllipse(
                    null,
                    new Pen(Brushes.Blue, 1),
                    new Point(agent.Node.X, agent.Node.Y),
                    agent.Radius,
                    agent.Radius);
                
                //出口までの経路の描画
                foreach(var node in agent.RouteNode)
                {
                    DrawContext.DrawLine(
                        new Pen(Brushes.Red, 10),
                        new Point(node.PreviousNode.X, node.PreviousNode.Y),
                        new Point(node.X, node.Y));
                }
            }

            DrawContext.Close();

            //表示する画像を更新 
            Bitmap.Render(DrawVisual);
        }
    }
}
