//*************************************************************************
//	创建日期:	2017-2-5
//	文件名称:	CMapObject.cs
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
    //-------------------------------------------------------------------------
    public class EICCMapObject
    {
        public EICBox vbox;
        public int?[] color;

        public EICCMapObject(EICBox vbox, int?[] color)
        {
            this.vbox = vbox;
            this.color = color;
        }
    }
    //-------------------------------------------------------------------------
}
