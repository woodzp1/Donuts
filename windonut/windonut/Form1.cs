using System.Numerics;

namespace windonut
{
    public partial class Form1 : Form
    {
        //private Bitmap donut;
        private float time = 0f;
        private System.Windows.Forms.Timer timer;
        public List<Rectangle> quads = new List<Rectangle>();
        private readonly Random rng = new Random();
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Greedy(Hit(time));



            timer = new System.Windows.Forms.Timer();
            timer.Interval = 33;
            timer.Tick += Timer_Tick;
            timer.Start();

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            time += 0.05f;


            Greedy(Hit(time));


            Invalidate();
        }


        public static bool[,] Hit(float time)
        {

            Rectangle r = Screen.PrimaryScreen.WorkingArea;

            Point size = new Point(r.Width, r.Height);
            Point march_size = new Point(60, 60);
            bool[,] hits = new bool[march_size.X, march_size.Y];
            for (int x = 0; x < march_size.X; x++)
            {
                for (int y = 0; y < march_size.Y; y++)
                {
                    Vector2 uv = new Vector2((float)x / march_size.X, (float)y / march_size.Y);
                    uv = uv * 2.0f - new Vector2(1, 1);

                    Vector3 rd = Vector3.Normalize(new Vector3(uv.X, uv.Y, 1));
                    Vector3 ro = new Vector3(0, 0, -5.3f);

                    float d = RayMarch(rd, ro, time);
                    if (d > 0)
                    {
                        hits[x, y] = true;
                    }
                    else
                    {
                        hits[x, y] = false;
                    }


                }
            }


            return hits;
        }
        //static Bitmap draw(bool[,] hits)
        //{
        //    Bitmap img = new Bitmap(hits.GetLength(0), hits.GetLength(1), System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        //    for (int x = 0; x < hits.GetLength(0); x++)
        //    {
        //        for (int y = 0; y < hits.GetLength(1); y++)
        //        {
        //            if (hits[x, y])
        //            {
        //                img.SetPixel(x, y, Color.White);
        //            }
        //            else
        //            {
        //                img.SetPixel(x, y, Color.Black);
        //            }
        //        }
        //    }
        //    return img;



        //}

        static float RayMarch(Vector3 rd, Vector3 ro, float time)
        {
            Matrix4x4 rotation = Matrix4x4.CreateRotationX(time) * Matrix4x4.CreateRotationY(time * 0.5f) * Matrix4x4.CreateRotationX(time * 0.3f);
            float t = 0;
            int iterations = 40;
            Matrix4x4 rot = Matrix4x4.CreateRotationX(time);

            for (int i = 0; i < iterations; i++)
            {

                Vector3 p = ro + rd * t;
                p = Vector3.Transform(p, rotation);

                float temp = Map(p, time);
                float d = temp;

                if (d < 0.01f)
                {
                    return t;
                }
                if (t > 100.0f)
                {
                    return -1.0f;
                }
                t += d;

            }


            return -1.0f;

        }
        static float Map(Vector3 pos, float time)
        {


            // rad , thickness
            Vector2 t = new Vector2(2.8f, 1.0f);
            Vector3 ofset = new Vector3(0, 0, 5);
            float torus = SdfTaurus(pos, t);
            return torus;
        }
        static float SdfTaurus(Vector3 pos, Vector2 t)
        {

            Vector2 temp = new Vector2(pos.X, pos.Z);
            Vector2 q = new Vector2(temp.Length() - t.X, pos.Y);

            return q.Length() - t.Y;


        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {


            //Color color = Color.FromArgb(rng.Next(256), rng.Next(256), rng.Next(256));
            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new(brush);

            foreach (Rectangle item in quads)
            {

                e.Graphics.DrawRectangle(pen, item);
            }

        }

        public void Greedy(bool[,] hits)
        {
            quads.Clear();
            while (true)
            {
                Point Get_start()
                {
                    Point start = new Point();
                    for (int y = 0; y < hits.GetLength(1); y++)
                        for (int x = 0; x < hits.GetLength(0); x++)
                        {

                            if (hits[x, y])
                            {
                                start = new Point(x, y);
                                return start;
                            }

                        }
                    return start;
                }
                Point start = Get_start();
                if (start.IsEmpty)
                {
                    return;
                }
                bool hit = true;
                int c = 0;
                while (start.X + c < hits.GetLength(0) && hits[start.X + c, start.Y])
                {
                    c++;
                    //hit = hits[start.X + c, start.Y];

                }
                int v = 0;
                hit = true;
                bool rowOk = true;
                while (rowOk && start.Y + v < hits.GetLength(1))
                {
                    for (int x = 0; x < c; x++)
                    {
                        if (!hits[start.X + x, start.Y + v])
                        {
                            rowOk = false;
                            break;
                        }
                    }
                    if (rowOk)
                    {
                        v++;
                    }
                }

                Rectangle r = new Rectangle(start.X, start.Y, c * 10, v * 10);
                r.Offset(start.X * 9, start.Y * 9);

                quads.Add(r);
                for (int x = start.X; x < c + start.X; x++)
                {
                    for (int y = start.Y; y < v + start.Y; y++)
                    {
                        hits[x, y] = false;
                    }
                }



            }

        }



    }
}
