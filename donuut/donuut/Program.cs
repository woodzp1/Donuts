using System.Numerics;
using System.Text;
using static System.Formats.Asn1.AsnWriter;

namespace donuut
{
    internal class Program
    {
        static void Main(string[] args)
        {







            bool smooth = false;
            bool use_edges = true;
            bool use_ball = true;
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            int x_size = Console.WindowWidth;
            int y_size = Console.WindowHeight -1;
            bool[,] hit_arr = new bool[x_size, y_size];

            //char filledSquare = '\u25A0';
            //float[,] distances = new float[x_size, y_size];
            //int[,] iters = new int[x_size, y_size];
            //float min = 0.0f;
            //float max = 0.0f;
            float time = 0.0f;
            StringBuilder frame_buffer = new StringBuilder();
            //".:-=+*#@"
            //.:-=+*{filledSquare}#@o

            //string slopes = @"/|\-/|\-";
            string slopes = @"─┘┌││┐┐──┌┘││└└──";


            string gradient;
            //double[] gradient_val = { 0.0751, 0.0829, 0.0848, 0.1227, 0.1403, 0.1559, 0.185, 0.2183, 0.2417, 0.2571, 0.2852, 0.2902, 0.2919, 0.3099, 0.3192, 0.3232, 0.3294, 0.3384, 0.3609, 0.3619, 0.3667, 0.3737, 0.3747, 0.3838, 0.3921, 0.396, 0.3984, 0.3993, 0.4075, 0.4091, 0.4101, 0.42, 0.423, 0.4247, 0.4274, 0.4293, 0.4328, 0.4382, 0.4385, 0.442, 0.4473, 0.4477, 0.4503, 0.4562, 0.458, 0.461, 0.4638, 0.4667, 0.4686, 0.4693, 0.4703, 0.4833, 0.4881, 0.4944, 0.4953, 0.4992, 0.5509, 0.5567, 0.5569, 0.5591, 0.5602, 0.5602, 0.565, 0.5776, 0.5777, 0.5818, 0.587, 0.5972, 0.5999, 0.6043, 0.6049, 0.6093, 0.6099, 0.6465, 0.6561, 0.6595, 0.6631, 0.6714, 0.6759, 0.6809, 0.6816, 0.6925, 0.7039, 0.7086, 0.7235, 0.7302, 0.7332, 0.7602, 0.7834, 0.8037, 0.9999 };
            if (!smooth)
            {
                gradient = ".:!-=x+*uG$&#%0@@";
            }
            else
            {
                gradient = "`.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@";
            }

            Console.WriteLine($"Use Edges?");
            use_edges = (char.ToLower(Console.ReadLine()?.FirstOrDefault() ?? 'y') == 'y') ? true : false;
            Console.WriteLine("Use ball?");
            use_ball = (char.ToLower(Console.ReadLine()?.FirstOrDefault() ?? 'y') == 'y') ? true : false;
            Console.WriteLine("Press ctrl + c to exit");
            Thread.Sleep(1000);
            while (true)
            {
                if (x_size != Console.WindowWidth)
                {
                    x_size = Console.WindowWidth;
                    y_size = Console.WindowHeight - 1;
                    hit_arr = new bool[x_size, y_size];

                }
                
                time += 0.03f;
                frame_buffer.Clear();
                for (int y = 0; y < y_size; y++)
                {
                    for (int x = 0; x < x_size; x++)
                    {

                        float u = (((float)x / x_size) * 2.0f - 1.0f) * ((float)x_size / y_size) * 0.5f;
                        float v = ((float)y / y_size) * 2.0f - 1.0f;


                        Vector3 rd = Vector3.Normalize(new Vector3(u, v, 1.3f));
                        int iter = 0;
                        Vector3 camera = new Vector3(0, 0, -5.3f);


                        Vector3 light = new Vector3(-1.0f, 0.0f, 0);
                        light = Vector3.Normalize(light);


                        Vector3 norm;
                        float distance = RayMarch(rd, camera, time, out iter, out norm,use_ball);
                        if (distance > 0)
                        {


                            float intensity = (Vector3.Dot(light, norm) + 1.0f) * 0.5f;

                            intensity = Math.Clamp(intensity, 0, 1);
                            int index = Math.Clamp((int)(intensity * gradient.Length), 0, gradient.Length - 1);

                            hit_arr[x, y] = true;



                            frame_buffer.Append(gradient[index]);

                        }
                        else
                        {
                            try
                            {
                                hit_arr[x, y] = false;
                                frame_buffer.Append(' ');
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            
                        }


                    }
                    frame_buffer.Append('\n');
                }
                if (use_edges)
                {
                    for (int y = 0; y < y_size; y++)
                    {
                        for (int x = 0; x < x_size; x++)
                        {
                            int str_index = x + y * x_size;

                            float angle = 0;
                            if (hit_arr[x, y])
                            {
                                angle = sobel(new Vector2(x, y), hit_arr);
                            }
                            if (angle > 0.005f) // Threshold to filter out noise
                            {

                                int index = (int)(angle * (slopes.Length - 1));
                                frame_buffer[str_index] = slopes[index];
                            }



                        }

                    }

                }
                Console.SetCursorPosition(0, 0);
                Console.Write(frame_buffer.ToString());
                Thread.Sleep(1);
            }
            static float RayMarch(Vector3 rd, Vector3 ro, float time, out int iter, out Vector3 norm,in bool use_ball)
            {
                
                Matrix4x4 rotation = Matrix4x4.CreateRotationX(time * 0.8f) * Matrix4x4.CreateRotationY(time * 0.6f) * Matrix4x4.CreateRotationX(time * 0.3f);
                float t = 0;
                int iterations = 40;
                Matrix4x4 rot = Matrix4x4.CreateRotationX(time);
                for (int i = 0; i < iterations; i++)
                {
                    iter = i;
                    Vector3 p = ro + rd * t;
                    p = Vector3.Transform(p, rotation);
                    bool shape = false;
                    float temp = Map(p, time, out shape, in use_ball);
                    float d = temp;
                    norm = new Vector3(0, 0, 0);
                    if (d < 0.01f)
                    {
                        Vector2 temp_vec = new Vector2(2.8f, 0.7f);
                        if (shape)
                        {
                            temp_vec = new Vector2(0, 1);
                        }
                        norm = TaurusNormal(p, temp_vec);
                        Matrix4x4.Invert(rotation, out rot);
                        norm = Vector3.Transform(norm, rot);
                        return t;
                    }
                    if (t > 100.0f)
                    {
                        return -1.0f;
                    }
                    t += d;

                }
                norm = new Vector3(0.0f, 0.0f, 0.0f);
                iter = -1;
                return t;

            }

            static float Map(Vector3 pos, float time, out bool circle, in bool use_ball)
            {
                circle = false;
                Vector3 cube_pos = pos - new Vector3(0, (float)Math.Sin(time) * 3, 0);
                Matrix4x4 rotation = Matrix4x4.CreateRotationX(time) * Matrix4x4.CreateRotationZ(time * 0.5f) * Matrix4x4.CreateRotationY(time * 0.3f);
                //pos = Vector3.Transform(pos, rotation);
                // rad , thickness
                Vector2 t = new Vector2(2.8f, 0.7f);

                //Vector3 box = new(4, 4, 4);
                Vector3 ofset = new Vector3(0, 0, 5);
                float torus = SdfTaurus(pos, t);
                float cube = SdfTaurus(cube_pos, new Vector2(0, 1));
                if (!use_ball)
                {
                    return torus;
                }
                //return cube;

                circle = torus > cube;

                return Math.Min(torus, cube);
            }
            static float SdfTaurus(Vector3 pos, Vector2 t)
            {

                Vector2 temp = new Vector2(pos.X, pos.Z);
                Vector2 q = new Vector2(temp.Length() - t.X, pos.Y);

                return q.Length() - t.Y;


            }
            static Vector3 TaurusNormal(Vector3 pos, Vector2 t)
            {


                Vector2 xzDir = Vector2.Normalize(new Vector2(pos.X, pos.Z));

                Vector3 closestPointOnCircle = new Vector3(xzDir.X * t.X, 0, xzDir.Y * t.X);
                return Vector3.Normalize(pos - closestPointOnCircle);

            }

            static float sobel(Vector2 coord, bool[,] hit)
            {
                int[,] gxKernel = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
                int[,] gyKernel = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

                coord = Vector2.Clamp(coord, new Vector2(1, 1), new Vector2(118, 27));
                float gx = 0;
                float gy = 0;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {

                        float c = hit[(int)coord.X + i, (int)coord.Y + j] ? 100.0f : 0.0f;
                        gx += gxKernel[i + 1, j + 1] * c;
                        gy += gyKernel[i + 1, j + 1] * c;
                        gy *= 1;
                    }
                }
                float tg = (float)Math.Atan2(gy, gx);
                float magnitude = (float)Math.Sqrt((gx * gx) + (gy * gy));
                tg = (tg / (float)Math.PI + 1.0f) * 0.5f;
                tg *= Math.Clamp(magnitude, 0, 1);


                return tg;
            }
        }
    }
}
