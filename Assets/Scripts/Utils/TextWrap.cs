using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public static class TextWrap
    {
        public static string WrapText(string input, int maxCharsPerLine)
        {
            var words = input.Split(' ');
            var sb = new System.Text.StringBuilder();
            int lineLength = 0;

            foreach (var word in words)
            {
                if (lineLength + word.Length + 1 > maxCharsPerLine)
                {
                    sb.Append("\n");
                    lineLength = 0;
                }
                else if (lineLength > 0)
                {
                    sb.Append(" ");
                    lineLength += 1;
                }

                sb.Append(word);
                lineLength += word.Length;
            }

            return sb.ToString();
        }
    }
}