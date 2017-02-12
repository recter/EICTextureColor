# EICTextureColor

---

纹理分色 For Unity，这是一个简单的纹理分色小工具。

# 使用样例

<pre>
    //-------------------------------------------------------------------------
    private void __ParserTextureColor()
    {
        if (null == m_txtMain)
        {
            return;
        }

        List<int?[]> pColors = EICTextureColor.GetPalette(m_txtMain as Texture2D);
        if (null == pColors || 0 == pColors.Count)
        {
            return;
        }

        List<Color> txtColors = new List<Color>();
        List<string> strColors = new List<string>();
        foreach (var v in pColors)
        {
            if (null == v || 3 < v.Length)
            {
                continue;
            }
            txtColors.Add(new Color((float)v[0] / 255.0f, (float)v[1] / 255.0f, (float)v[2] / 255.0f));
            strColors.Add(__GetColor(v));
        }
        StringBuilder sb = new StringBuilder();

        int nIndex = 0;
        sb.AppendLine("该纹理的主要颜色为：");
        foreach (var v in txtColors)
        {
            sb.Append("<color=#" + strColors[nIndex ++ ] + ">");
            sb.Append(v.ToString());
            sb.Append("</color>,");
        }
        Debug.LogWarning(sb.ToString());

        if (null != m_text)
        {
            m_text.text = sb.ToString();
        }

    }
    //-------------------------------------------------------------------------
</pre>

# License
The MIT License (MIT)

Copyright (c) 2015 shadowkong.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.