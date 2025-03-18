using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Standard.Helpers
{
    public static class ColorHelper
    {
        /// <summary>
        /// 生成指定数量的高区分度颜色
        /// </summary>
        /// <param name="count">调整 count 参数即可适应不同需求</param>
        /// <param name="seed"> seed 参数控制随机性，方便调试和一致性需求</param>
        /// <returns></returns>
        public static List<Color> GenerateDistinctColors(int count, int seed = 0)
        {
            Random random = seed == 0 ? new Random() : new Random(seed);
            List<Color> colors = new List<Color>();

            for (int i = 0; i < count; i++)
            {
                // 计算色相（均匀分布）
                double hue = (i * (360.0 / count)) % 360;

                // 高饱和度（80%~100%）
                double saturation = random.Next(80, 101);

                // 低亮度（20%~40%）：颜色更暗但保留鲜艳度
                double lightness = random.Next(40, 41);

                // 转换为RGB颜色
                Color color = HSLToRGB(hue, saturation, lightness);
                colors.Add(color);
            }

            return colors;
        }

        /// <summary>
        /// HSL转RGB核心函数
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        private static Color HSLToRGB(double h, double s, double l)
        {
            h %= 360;
            s = Math.Min(100, Math.Max(0, s));
            l = Math.Min(100, Math.Max(0, l));

            double c = (1 - Math.Abs(2 * l / 100 - 1)) * (s / 100);
            double hPrime = h / 60;
            double x = c * (1 - Math.Abs(hPrime % 2 - 1));
            double r1, g1, b1;

            if (0 <= hPrime && hPrime < 1)
            {
                r1 = c; g1 = x; b1 = 0;
            }
            else if (1 <= hPrime && hPrime < 2)
            {
                r1 = x; g1 = c; b1 = 0;
            }
            else if (2 <= hPrime && hPrime < 3)
            {
                r1 = 0; g1 = c; b1 = x;
            }
            else if (3 <= hPrime && hPrime < 4)
            {
                r1 = 0; g1 = x; b1 = c;
            }
            else if (4 <= hPrime && hPrime < 5)
            {
                r1 = x; g1 = 0; b1 = c;
            }
            else
            {
                r1 = c; g1 = 0; b1 = x;
            }

            double m = l / 100 - c / 2;
            byte r = (byte)Math.Round((r1 + m) * 255);
            byte g = (byte)Math.Round((g1 + m) * 255);
            byte b = (byte)Math.Round((b1 + m) * 255);

            return Color.FromArgb(255, r, g, b);
        }
    }
}
