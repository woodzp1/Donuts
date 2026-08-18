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
        public bool focus = true;

        const int MAX_FORMS = 25;
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;

            StartPosition = FormStartPosition.CenterScreen;


            timer = new System.Windows.Forms.Timer();
            timer.Interval = 33;
            timer.Tick += Timer_Tick;
            timer.Start();

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            time += 0.05f;
            if (focus)
            {
                Rects(Hit(time));
                Display_Forms();
            }


            //Invalidate();
        }

        #region raymarch

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

        #endregion
        //private void Form1_Paint(object sender, PaintEventArgs e)
        //{


        //    //Color color = Color.FromArgb(rng.Next(256), rng.Next(256), rng.Next(256));
        //    Brush brush = new SolidBrush(Color.Black);
        //    Pen pen = new(brush);
        //    quads = quads.OrderByDescending(p => (p.Width * p.Height)).Take(30).ToList();


        //    foreach (Rectangle item in quads)
        //    {

        //        e.Graphics.DrawRectangle(pen, item);
        //    }

        //}







        public static Rectangle FindMaxRectangle(bool[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            //{ Top = 0, Left = 0, Bottom = -1, Right = -1 }

            Rectangle best = new Rectangle(0, 0, 0, 0); // area 0
            if (rows == 0 || cols == 0)
                return best;

            int[] heights = new int[cols];

            for (int r = 0; r < rows; r++)
            {
                // Update histogram heights for this row
                for (int c = 0; c < cols; c++)
                    heights[c] = grid[r, c] ? heights[c] + 1 : 0;

                // Largest rectangle in this histogram, with a sentinel 0-height pass at c == cols
                var stack = new Stack<int>();
                for (int c = 0; c <= cols; c++)
                {
                    int h = (c == cols) ? 0 : heights[c];

                    while (stack.Count > 0 && heights[stack.Peek()] >= h)
                    {
                        int idx = stack.Pop();
                        int height = heights[idx];
                        int left = stack.Count == 0 ? 0 : stack.Peek() + 1;
                        int right = c - 1;
                        int width = right - left + 1;
                        int area = height * width;

                        if (area > best.Height * best.Width)
                        {
                            best = new Rectangle(left, r - height + 1, width, height);
                            //best = new Rectangle
                            //{

                            //    Top = r - height + 1,
                            //    Bottom = r,
                            //    Left = left,
                            //    Right = right
                            //};
                        }
                    }
                    stack.Push(c);
                }
            }

            return best;
        }
        public void Rects(bool[,] hits)
        {

            quads.Clear();
            for (int v = 0; v < MAX_FORMS; v++)
            {
                Rectangle best = FindMaxRectangle(hits);
                if (best.Width * best.Height == 0)
                    break;

                for (int j = best.Y; j < best.Bottom; j++)
                {
                    for (int i = best.X; i < best.Right; i++)
                    {
                        hits[j, i] = false;
                    }
                }

                best.Offset(10, 10);
                best.Width *= 10;
                best.Height *= 10;
                best.X *= 10;
                best.Y *= 10;
                quads.Add(best);




            }
        }
        private List<Form> form_pool = new List<Form>();
        public void Display_Forms()
        {
            while (form_pool.Count < MAX_FORMS)
            {
                Form form = new No_Active_Form
                {
                    StartPosition = FormStartPosition.Manual,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    ShowInTaskbar = false,
                    FormBorderColor = Color.Black,
                    ControlBox = false,
                    Owner = this
                    



                };
                
                form_pool.Add(form);
            }
            
            for (int i = 0; i < form_pool.Count; i++)
            {
                Form form = form_pool[i];
                if (i < quads.Count)
                {
                    Rectangle rect = quads[i];

                    form.SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
                    if (!form.Visible)
                    {
                        form.Show();
                    }
                }
                else
                {
                    if (form.Visible)
                    {
                        form.Hide();
                    }
                }



            }


        }
        
        private void DisposeFormPool()
        {
            foreach (Form item in form_pool)
            {
                item.Close();
                item.Dispose();
            }
            form_pool.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeFormPool();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            focus = true;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {

            focus = false;
            DisposeFormPool();
        }
    }
    //had an issue with focus this fixes it
    public class No_Active_Form : Form
    {
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

    }
}
