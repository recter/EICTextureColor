//*************************************************************************
//	创建日期:	2017-2-5
//	文件名称:	VBox.cs
//  创建作者:	Rect 	
//	版权所有:	shadowkong.com
//	相关说明:	移植自 - http://www.cnblogs.com/xinglizhenchu/p/5164816.html
//*************************************************************************

//-------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace EIC
{
    public class EICBox
    {
        //-------------------------------------------------------------------------
        public EICBox(int r1, int r2, int g1, int g2, int b1, int b2, int[] histo)
        {
            this.r1 = r1;
            this.r2 = r2;
            this.g1 = g1;
            this.g2 = g2;
            this.b1 = b1;
            this.b2 = b2;

            this.histo = histo;
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 设置参数或者
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int[] this[string s]
        {
            get
            {
                if (s == "r") return new int[] { r1, r2 };
                if (s == "g") return new int[] { g1, g2 };
                if (s == "b") return new int[] { b1, b2 };

                //不会发生
                return null;
            }
            set
            {
                if (s == "r")
                {
                    r1 = value[0];
                    r2 = value[1];
                }
                if (s == "g")
                {
                    g1 = value[0];
                    g2 = value[1];
                }
                if (s == "b")
                {
                    b1 = value[0];
                    b2 = value[1];
                }

            }
        }
        //-------------------------------------------------------------------------
        private int? _volume;
        /// <summary>
        /// 得到颜色空间的体积
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public int volume(bool force = false)
        {
            if (!this._volume.HasValue || force)
                this._volume = (r1 - r2 + 1) * (g1 - g2 + 1) * (b1 - b2 + 1);
            return this._volume.Value;
        }
        //-------------------------------------------------------------------------
        bool _count_set = false;
        int _count = 0;
        //-------------------------------------------------------------------------
        /// <summary>
        /// 得到空间的点政数量
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public int count(bool force = false)
        {
            if (!_count_set || force)
            {
                int npix = 0,
                    i, j, k;
                for (i = r1; i <= r2; i++)
                {
                    for (j = g1; j <= g2; j++)
                    {
                        for (k = b1; k <= b2; k++)
                        {
                            var index = EICTextureColor.GetColorIndex(i, j, k);
                            npix += index < histo.Length ? histo[index] : 0;
                        }
                    }
                }
                _count = npix;
                _count_set = true;
            }
            return _count;
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 对象复制
        /// </summary>
        /// <returns></returns>
        public EICBox copy()
        {
            //返回新的VBOX
            return new EICBox(r1, r2, g1, g2, b1, b2, histo);
        }
        //-------------------------------------------------------------------------
        public int?[] _avg;
        public int?[] avg(bool force = false)
        {
            var vbox = this;
            var histo = vbox.histo;
            if (vbox._avg == null || force)
            {
                int ntot = 0,
                    mult = 1 << (8 - EICTextureColor.sigbits);

                float rsum = 0,
                   gsum = 0,
                   bsum = 0;
                int hval,
                  i, j, k, histoindex;
                for (i = vbox.r1; i <= vbox.r2; i++)
                {
                    for (j = vbox.g1; j <= vbox.g2; j++)
                    {
                        for (k = vbox.b1; k <= vbox.b2; k++)
                        {
                            histoindex =
                                EICTextureColor.GetColorIndex(i, j, k);
                            hval = histoindex < histo.Length ? histo[histoindex] : 0;
                            ntot += hval;
                            rsum += (hval * (i + 0.5f) * mult);
                            gsum += (hval * (j + 0.5f) * mult);
                            bsum += (hval * (k + 0.5f) * mult);
                        }
                    }
                }
                if (ntot > 0)
                {
                    vbox._avg = new int?[] {
                        ~~(int)(rsum / ntot),
                        ~~(int)(gsum / ntot),
                        ~~(int)(bsum / ntot)
                    };
                }
                else
                {
                    vbox._avg = new int?[]{
                        ~~(mult * (vbox.r1 + vbox.r2 + 1) / 2),
                        ~~(mult * (vbox.g1 + vbox.g2 + 1) / 2),
                        ~~(mult * (vbox.b1 + vbox.b2 + 1) / 2)
                    };
                }
            }
            return vbox._avg;
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// 判断像素是否在矩阵范围内
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        public bool contains(int[] pixel)
        {
            var vbox = this;
            int rval = pixel[0] >> EICTextureColor.rshift;
            int gval = pixel[1] >> EICTextureColor.rshift;
            int bval = pixel[2] >> EICTextureColor.rshift;
            return (rval >= vbox.r1 && rval <= vbox.r2 &&
                    gval >= vbox.g1 && gval <= vbox.g2 &&
                    bval >= vbox.b1 && bval <= vbox.b2);
        }
        //-------------------------------------------------------------------------
        public int b2 { get; set; }
        public int b1 { get; set; }

        public int g2 { get; set; }
        public int g1 { get; set; }

        public int r2 { get; set; }
        public int r1 { get; set; }
        public int[] histo { get; set; }
    }
}
