//*************************************************************************
//	创建日期:	2017-2-5
//	文件名称:	PQueue.cs
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
    public class EICPQueue<T>
    {
        List<T> contents = new List<T>();
        bool sorted = false;

        //public delegate int Comparartor(VBox a, VBox b);

        //-------------------------------------------------------------------------
        public EICPQueue(Func<T, T, int> comparator)
        {
            this.comparator = comparator;
        }
        //-------------------------------------------------------------------------
        public T this[int index]
        {
            get
            {
                return contents.ElementAt(index);
            }
        }
        //-------------------------------------------------------------------------
        public void push(T o)
        {
            contents.Add(o);
            sorted = false;
        }
        //-------------------------------------------------------------------------
        void sort()
        {
            contents.Sort(new Comparison<T>(comparator));
            sorted = true;
        }
        //-------------------------------------------------------------------------
        public void sort(Func<T, T, int> _comparator)
        {
            contents.Sort(new Comparison<T>(_comparator));
            sorted = true;
        }
        //-------------------------------------------------------------------------
        public T peek(int index = -1)
        {
            if (!sorted) sort();
            if (index == -1)
                index = contents.Count() - 1;
            return contents[index];
        }
        //-------------------------------------------------------------------------
        public T pop()
        {
            if (!sorted) sort();
            if (contents.Count > 0)
            {
                var obj = contents.ElementAt<T>(0);
                contents.RemoveAt(0);
                return obj;
            }
            return default(T);
        }
        //-------------------------------------------------------------------------
        public int size()
        {
            return contents.Count();
        }
        //-------------------------------------------------------------------------
        public List<T1> map<T1>(Func<T, T1> f)
        {
            var list = new List<T1>();
            foreach (var o in contents)
            {
                list.Add(f(o));
            }
            return list;
        }
        //-------------------------------------------------------------------------
        public Func<T, T, int> comparator { get; set; }
    }
}
