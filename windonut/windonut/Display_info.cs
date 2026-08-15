using System;
using System.Collections.Generic;
using System.Text;

namespace windonut
{
    internal class Display_info
    {
        private Point position;
        private Point size;
        private int area;

        public Display_info(Point position, Point size, int area)
        {
            this.position = position;
            this.size = size;
            this.area = area;
        }

        public Point Position { get => position; set => position = value; }
        public Point Size { get => size; set => size = value; }
        public int Area { get => area; set => area = value; }
    }
}
