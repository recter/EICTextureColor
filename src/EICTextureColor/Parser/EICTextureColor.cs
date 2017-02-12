//*************************************************************************
//	创建日期:	2017-2-5
//	文件名称:	EICTextureColor.cs
//  创建作者:	Rect 	
//	版权所有:	shadowkong.com
//	相关说明:	移植自 - http://www.cnblogs.com/xinglizhenchu/p/5164816.html
//*************************************************************************

//-------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace EIC
{
    /// <summary>
    /// 用于图像分色
    /// </summary>
    public class EICTextureColor
    {
        #region Member variables
        //-------------------------------------------------------------------------
        public const int sigbits = 5;
        public const int rshift = 8 - sigbits;
        public const int maxIterations = 1000;
        public const float fractByPopulations = 0.75f;
        //-------------------------------------------------------------------------
        #endregion

        #region Public Method
        //-------------------------------------------------------------------------
        public static int GetColorIndex(int r, int g, int b)
        {
            return (r << (2 * sigbits)) + (g << sigbits) + b;
        }

        /// <summary>
        /// 得到第一个颜色
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static int?[] GetColor(Texture2D sourceImage, int quality = 10)
        {
            if (null == sourceImage)
            {
                return null;
            }
            var palette = GetPalette(sourceImage, 5, quality);
            var dominantColor = palette[0];
            return dominantColor;
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 得到颜色数组
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="colorCount">颜色数量</param>
        /// <param name="quality">多少个像素进行分割</param>
        /// <returns></returns>
        public static List<int?[]> GetPalette(Texture2D sourceImage, int colorCount = 10, int quality = 10)
        {
            if (null == sourceImage)
            {
                return null;
            }

            //颜色数量
            if (colorCount < 1)
            {
                colorCount = 10;
            }
            //多少个像素进行分割
            if (quality < 1)
            {
                quality = 10;
            }

            var pixels = new List<Color>();
            for (int i = 0; i < sourceImage.height; i++)
            {
                for (int j = 0; j < sourceImage.width; j++)
                {
                    pixels.Add(sourceImage.GetPixel(j, i));
                }
            }

            var pixelCount = pixels.Count;

            var pixelArray = new List<Color>();

            //间隔进行像素点采样，每个像素点都包含a r g b 四个值（a为透明度）
            for (int i = 0; i < pixelCount; i = i + quality)
            {
                var color = pixels[i];
                // If pixel is mostly opaque and not white
                if ((color.a * 255) >= 125)
                {
                    //大于白色都去掉
                    if (!((color.r * 255) > 250 && (color.g * 255) > 250 && (color.b * 255) > 250))
                    {
                        //加入到对象里面，这里面就是rgb的值
                        pixelArray.Add(color);
                    }
                }
            }

            var cmap = __Quantize(pixelArray, colorCount);


            List<int?[]> palette = cmap != null ? cmap.palette<int?[]>() : null;

            // Clean up

            return palette;
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int NaturalOrder(int a, int b)
        {
            return a < b ? -1 : (a > b ? 1 : 0);
        }
        //-------------------------------------------------------------------------
        #endregion

        #region private Method
        //-------------------------------------------------------------------------
        /// <summary>
        /// 这段代码？切分矩阵
        /// </summary>
        /// <param name="partialsum"></param>
        /// <param name="lookaheadsum"></param>
        /// <param name="vbox"></param>
        /// <param name="color"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private static EICBox[] __DoCut(Dictionary<int, int?> partialsum,
            Dictionary<int, int?> lookaheadsum,
            EICBox vbox, string color, int total)
        {
            int i,
                left, right, d2;
            EICBox vbox1, vbox2;

            //从一个维度的最小值到最大值，rgb。
            for (i = vbox[color][0]; i <= vbox[color][1]; i++)
            {
                //代表里面的命中的像素以及大于整体的一半信息。
                if (partialsum[i] > total / 2)
                {
                    vbox1 = vbox.copy();
                    vbox2 = vbox.copy();

                    left = i - vbox[color][0];
                    right = vbox[color][1] - i;

                    if (left <= right)
                        d2 = Math.Min(vbox[color][1] - 1, ~~(i + right / 2));
                    else
                        d2 = Math.Max(vbox[color][0], ~~(i - 1 - left / 2));

                    while (!partialsum[d2].HasValue) d2++;
                    //肯定有内容
                    var count2 = lookaheadsum[d2];
                    while (
                        (!count2.HasValue || count2.Value == 0)
                        && partialsum.ContainsKey(d2 - 1)
                        && partialsum[d2 - 1].HasValue
                        && partialsum[d2 - 1].Value > 0)
                        count2 = lookaheadsum[--d2];

                    // set dimensions
                    vbox1[color] = new int[] { vbox1[color][0], d2 };
                    vbox2[color] = new int[] { d2 + 1, vbox2[color][1] };

                    return new EICBox[] { vbox1, vbox2 };
                }
            }

            return null;

        }
        //-------------------------------------------------------------------------
        private static object[] __MedianCutApply(int[] histo, EICBox vbox)
        {
            if (vbox.count() == 0) return null;

            int rw = vbox.r2 - vbox.r1 + 1,
                gw = vbox.g2 - vbox.g1 + 1,
                bw = vbox.b2 - vbox.b1 + 1,

                //最大颜色，按照最大的变进行切分
                maxw = Math.Max(Math.Max(rw, gw), bw);


            // only one pixel, no split
            if (vbox.count() == 1)
            {
                return new EICBox[] { vbox.copy(), null };
            }
            /* Find the partial sum arrays along the selected axis. */
            int total = 0;
            Dictionary<int, int?> partialsum = new Dictionary<int, int?>();
            Dictionary<int, int?> lookaheadsum = new Dictionary<int, int?>();
            int i, j, k, sum, index;

            if (maxw == rw)
            {
                for (i = vbox.r1; i <= vbox.r2; i++)
                {
                    sum = 0;
                    for (j = vbox.g1; j <= vbox.g2; j++)
                    {
                        for (k = vbox.b1; k <= vbox.b2; k++)
                        {
                            index = GetColorIndex(i, j, k);
                            sum += index < histo.Length ? histo[index] : 0;
                        }
                    }
                    total += sum;
                    partialsum[i] = total;
                }
            }
            else if (maxw == gw)
            {
                for (i = vbox.g1; i <= vbox.g2; i++)
                {
                    sum = 0;
                    for (j = vbox.r1; j <= vbox.r2; j++)
                    {
                        for (k = vbox.b1; k <= vbox.b2; k++)
                        {
                            index = GetColorIndex(j, i, k);
                            sum += index < histo.Length ? histo[index] : 0;
                        }
                    }
                    total += sum;
                    partialsum[i] = total;
                }
            }
            else
            {  /* maxw == bw */
                for (i = vbox.b1; i <= vbox.b2; i++)
                {
                    sum = 0;
                    for (j = vbox.r1; j <= vbox.r2; j++)
                    {
                        for (k = vbox.g1; k <= vbox.g2; k++)
                        {
                            index = GetColorIndex(j, k, i);
                            sum += index < histo.Length ? histo[index] : 0;
                        }
                    }
                    total += sum;
                    partialsum[i] = total;
                }
            }

            foreach (var v in partialsum)
            {
                //计算剩余的部分
                lookaheadsum[v.Key] = total - v.Value;
            }


            // determine the cut planes
            return
                maxw == rw ? __DoCut(partialsum, lookaheadsum, vbox, "r", total) :
                maxw == gw ? __DoCut(partialsum, lookaheadsum, vbox, "g", total) :
                __DoCut(partialsum, lookaheadsum, vbox, "b", total);
        }
        //-------------------------------------------------------------------------
        //<PQueue<T>, Dictionary<int, int>, float>
        private static void __Iter<T>(EICPQueue<T> lh, int[] histo, float target)
        {
            var ncolors = 1;
            var niters = 0;
            T vbox;

            while (niters < maxIterations)
            {
                vbox = lh.pop();
                if ((vbox as EICBox).count() == 0)
                { /* just put it back */
                    lh.push(vbox);
                    niters++;
                    continue;
                }

                var vboxes = __MedianCutApply(histo, vbox as EICBox);

                T vbox1 = (T)vboxes[0];
                T vbox2 = (T)vboxes[1];

                if (vbox1 == null)
                {
                    //无法切分                    ("vbox1 not defined; shouldn't happen!");
                    return;
                }
                lh.push(vbox1);
                if (vbox2 != null)
                {  /* vbox2 can be null */
                    lh.push(vbox2);
                    ncolors++;
                }

                //找到最集中的点的数量，已经大于我们需要的，就不要在继续切分
                if (ncolors >= target) return;

                //循环的次数如果太多，也不要继续循环。
                if (niters++ > maxIterations)
                {
                    //                    ("infinite loop; perhaps too few pixels!");
                    return;
                }
            }
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 开始计算
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="maxcolors"></param>
        /// <returns></returns>
        private static EICCMap __Quantize(List<Color> pixels, int maxcolors)
        {

            if (pixels.Count == 0 || maxcolors < 2 || maxcolors > 256)
            {
                return null;
            }

            //得到颜色空间 里面每种颜色的数量集合
            var histo = __GetHisto(pixels);

            //color总数
            var nColors = 0;

            //代表总共有多少种颜色，通过采样出来的

            foreach (var v in histo)
            {
                if (v > 0) nColors++;
            }

            //如果颜色还少于需要计算的颜色，应该不现实？
            if (nColors <= maxcolors)
            {
                // XXX: generate the new colors from the histo and return
            }

            //得到颜色的三维空间中 三个向量的最大值 最小值
            var vbox = __VboxFromPixels(pixels, histo);

            var pq = new EICPQueue<EICBox>((a, b) =>
            {
                return NaturalOrder(a.count(), b.count());
            });
            pq.push(vbox);

            //按照像素点进行切分
            __Iter(pq, histo, fractByPopulations * maxcolors);

            //切分完毕的数据，按照重量进行排序
            var pq2 = new EICPQueue<EICBox>(
            (a, b) =>
            {
                return NaturalOrder(a.count() * a.volume(), b.count() * b.volume());
            });

            //把切分完毕的，装在进去
            while (pq.size() > 0)
            {
                pq2.push(pq.pop());
            }

            //?继续切分吗？
            __Iter(pq2, histo, maxcolors - pq2.size());


            // calculate the actual colors
            var cmap = new EICCMap();
            while (pq2.size() > 0)
            {
                cmap.push(pq2.pop());
            }

            return cmap;
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 得到矩阵中的顶点坐标
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="histo"></param>
        /// <returns></returns>
        private static EICBox __VboxFromPixels(List<Color> pixels, int[] histo)
        {
            int rmin = 1000000, rmax = 0,
                gmin = 1000000, gmax = 0,
                bmin = 1000000, bmax = 0,
                rval, gval, bval;

            // find min/max
            pixels.ForEach((o) =>
            {
                rval = (int)(o.r * 255) >> rshift;
                gval = (int)(o.g * 255) >> rshift;
                bval = (int)(o.b * 255) >> rshift;

                if (rval < rmin) rmin = rval;
                else if (rval > rmax) rmax = rval;
                if (gval < gmin) gmin = gval;
                else if (gval > gmax) gmax = gval;
                if (bval < bmin) bmin = bval;
                else if (bval > bmax) bmax = bval;
            });

            //返回所有像素中，rgb中分别的最大值和最小值
            return new EICBox(rmin, rmax, gmin, gmax, bmin, bmax, histo);
        }
        //-------------------------------------------------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        private static int[] __GetHisto(List<Color> pixels)
        {
            var histosize = 1 << (3 * sigbits);

            var histo = new int[histosize];// (histosize),
            int index, rval, gval, bval;

            pixels.ForEach((o) =>
            {
                rval = (int)(o.r * 255) >> rshift;
                gval = (int)(o.g * 255) >> rshift;
                bval = (int)(o.b * 255) >> rshift;
                index = GetColorIndex(rval, gval, bval);

                histo[index]++;
            });

            return histo;
        }
        //-------------------------------------------------------------------------
        internal static int sum(int?[] ints)
        {
            int t = 0;
            foreach (var o in ints)
                t += o.Value;

            return t;
        }
        //-------------------------------------------------------------------------
        #endregion

    }
}
