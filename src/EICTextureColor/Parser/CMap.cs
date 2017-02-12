//*************************************************************************
//	创建日期:	2017-2-5
//	文件名称:	CMap.cs
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

namespace EIC
{
    public class EICCMap
    {
        //-------------------------------------------------------------------------
        public EICPQueue<EICCMapObject> vboxes = new EICPQueue<EICCMapObject>((a, b) =>
        {
            return EICTextureColor.NaturalOrder(
                a.vbox.count() * a.vbox.volume(),
                b.vbox.count() * b.vbox.volume()
            );
        });
        //-------------------------------------------------------------------------
        /// <summary>
        /// 压入矩阵
        /// </summary>
        /// <param name="vbox"></param>
        public void push(EICBox vbox)
        {
            vboxes.push(new EICCMapObject(vbox, vbox.avg()));
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 抽取颜色
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> palette<T>()
        {
            return vboxes.map<T>(
                (o) =>
                {
                    return (T)(((o as EICCMapObject).color) as object);
                });
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 有多少矩阵
        /// </summary>
        /// <returns></returns>
        public int size()
        {
            return vboxes.size();
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 查找对应的颜色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public int?[] map(int[] color)
        {
            var vboxes = this.vboxes;

            for (var i = 0; i < vboxes.size(); i++)
            {
                //发现颜色点落到矩阵内部，就直接用这个颜色
                if (vboxes.peek(i).vbox.contains(color))
                {
                    return vboxes.peek(i).color;
                }
            }

            //否则就找聚合点
            return this.nearest(color);
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 计算vbox聚合中心点位置
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public int?[] nearest(int[] color)
        {
            var vboxes = this.vboxes;

            double d1 = -1, d2;
            int?[] pColor = null;
            for (var i = 0; i < vboxes.size(); i++)
            {
                d2 = Math.Sqrt(
                    Math.Pow((double)color[0] - (double)vboxes.peek(i).color[0], 2.0) +
                    Math.Pow((double)color[1] - (double)vboxes.peek(i).color[1], 2.0) +
                    Math.Pow((double)color[2] - (double)vboxes.peek(i).color[2], 2.0)
                );
                if (d2 < d1 || d1 == -1)
                {
                    d1 = d2;
                    pColor = vboxes.peek(i).color;
                }
            }
            return pColor;
        }
        //-------------------------------------------------------------------------
        void forcebw()
        {
            // XXX: won't  work yet
            var vboxes = this.vboxes;
            vboxes.sort((a, b) =>
            {
                return
                    EICTextureColor.NaturalOrder(
                    EICTextureColor.sum(a.color),
                    EICTextureColor.sum(b.color));
            });

            // force darkest color to black if everything < 5
            var lowest = vboxes[0].color;
            if (lowest[0] < 5 && lowest[1] < 5 && lowest[2] < 5)
                vboxes[0].color = new int?[] { 0, 0, 0 };

            // force lightest color to white if everything > 251
            var idx = vboxes.size() - 1;
            var highest = vboxes[idx].color;
            if (highest[0] > 251 && highest[1] > 251 && highest[2] > 251)
                vboxes[idx].color = new int?[] { 255, 255, 255 };
        }
        //-------------------------------------------------------------------------
    }
}
