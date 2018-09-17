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

                            SetUpNodes(new Node(
                                seats[0].PositionX - (seats[0].Width / 2) - distance,
                                seats[0].PositionY - (seats[0].Height / 2) - distance));
                            SetUpNodes(new Node(
                                seats[0].PositionX - (seats[0].Width / 2) - distance,
                                seats[0].PositionY + (seats[0].Height / 2) + distance));
                            SetUpNodes(new Node(
                                seats[seats.Count() - 1].PositionX + (seats[0].Width / 2) + distance,
                                seats[seats.Count() - 1].PositionY - (seats[0].Height / 2) - distance));
                            SetUpNodes(new Node(
                                seats[seats.Count() - 1].PositionX + (seats[0].Width / 2) + distance,
                                seats[seats.Count() - 1].PositionY + (seats[0].Height / 2) + distance));
                        }

                        //出口の設定
                        foreach (var goal in theater_.Goals)
                        {
                            nodes_.Add(new Node(goal.PositionX, goal.PositionY, true));
                        }

                        //経路の設定
                        foreach (var node1 in nodes_)
                        {
                            if (node1.IsGoalNode)
                            {
                                continue;
                            }

                            foreach (var node2 in nodes_)
                            {
                                if (node1 == node2)
                                {
                                    continue;
                                }

                                if (CheckAllSidesCollision(node1.X, node1.Y, node2.X, node2.Y, 25))
                                {
                                    node1.NextNodes.Add(node2);
                                }
                            }
                        }

                        foreach(var seats in theater_.Seats)
                        {
                            foreach(var seat in seats)
                            {
                                agents_.Add(new AgentBase(seat.PositionX, seat.PositionY - (seat.Height / 2) - 25));
                            }
                        }

                        DrawLayout();
                    }
                }
            }

        }

        /// <summary>
        /// ノードが他のノードの近くにあった時ノードを統合する
        /// </summary>
        /// <param name="node">ノード</param>
        public void SetUpNodes(Node node)
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
            foreach(var node1 in nodes_)
            {
                foreach (var node2 in node1.NextNodes)
                {
                    DrawContext.DrawLine(
                        new Pen(Brushes.Black, 1),
                        new Point(node1.X, node1.Y),
                        new Point(node2.X, node2.Y));
                }
            }

            //エージェントの描画
            foreach(var agent in agents_)
            {
                DrawContext.DrawEllipse(
                    null,
                    new Pen(Brushes.Blue, 1),
                    new Point(agent.X, agent.Y),
                    agent.Radius,
                    agent.Radius);
            }

            DrawContext.Close();

            //表示する画像を更新 
            Bitmap.Render(DrawVisual);
        }

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
        /// <param name="x1">ノード1のX座標</param>
        /// <param name="y1">ノード1のY座標</param>
        /// <param name="x2">ノード2のX座標</param>
        /// <param name="y2">ノード2のY座標</param>
        /// <param name="tolerance">許容範囲</param>
        /// <returns>
        /// true:衝突していない
        /// false:衝突している
        /// </returns>
        public bool CheckAllSidesCollision(double x1, double y1, double x2, double y2, double tolerance)
        {
            double theta = Math.Atan2(y2 - y1, x2 - x1);

            //上辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x1 - tolerance * Math.Sin(theta), y1 + tolerance * Math.Cos(theta)),
                new Point(x2 - tolerance * Math.Sin(theta), y2 + tolerance * Math.Cos(theta))))
            {
                return false;
            }

            //右辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x2 - tolerance * Math.Sin(theta), y2 + tolerance * Math.Cos(theta)),
                new Point(x2 + tolerance * Math.Sin(theta), y2 - tolerance * Math.Cos(theta))))
            {
                return false;
            }

            //下辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x2 + tolerance * Math.Sin(theta), y2 - tolerance * Math.Cos(theta)),
                new Point(x1 + tolerance * Math.Sin(theta), y1 - tolerance * Math.Cos(theta))))
            {
                return false;
            }

            //左辺
            if (CheckCollisionDeterminationWithAllSeatsAndSide(
                new Point(x1 + tolerance * Math.Sin(theta), y1 - tolerance * Math.Cos(theta)),
                new Point(x1 - tolerance * Math.Sin(theta), y1 + tolerance * Math.Cos(theta))))
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
        public bool CheckCollisionDeterminationWithAllSeatsAndSide(Point r1, Point r2)
        {
            bool t1, t2;

            foreach (var seats in theater_.Seats)
            {
                foreach (var seat in seats)
                {
                    var p1 = new Point(seat.PositionX - (seat.Width / 2), seat.PositionY - (seat.Height / 2));
                    var p2 = new Point(seat.PositionX + (seat.Width / 2), seat.PositionY - (seat.Height / 2));

                    //点を入れ替えて判定しないといけない
                    t1 = CheckCollisionSide(r1, r2, p1, p2);
                    t2 = CheckCollisionSide(p1, p2, r1, r2);
                    
                    if(t1 && t2)
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
        bool CheckCollisionSide(Point r1, Point r2, Point p1, Point p2)
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
