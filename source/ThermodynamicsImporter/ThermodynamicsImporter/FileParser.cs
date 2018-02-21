using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicsImporter
{
    internal class FileParser
    {
        string _source = "";
        string[] _lines;
        int _currentLineIndex = 0;

        internal FileParser(string source)
        {
            _source = source;
            _lines = _source.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            _currentLineIndex = 0;
        }

        internal string[] MergeNextLine(string[] currentLine)
        {
            var newTokens = ParseNextLine();
            if (newTokens != null)
            {
                return currentLine.Concat(newTokens).ToArray();
            }
            else
                throw new Exception("End-of-file reached while merging line");
        }
        internal bool IsNextLineContinuation()
        {

            if (_currentLineIndex < _lines.Length)
            {
                if (_lines[_currentLineIndex].Length > 0 && _lines[_currentLineIndex][0] == ' ')
                    return true;
                else
                    return false;
            }
            return false;

        }

        internal string[] ParseNextLine()
        {
            try
            {
                if (_currentLineIndex < _lines.Length)
                {
                    var tokens = _lines[_currentLineIndex].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    _currentLineIndex++;
                    return tokens;
                }
                return null;
            }
            catch (Exception e)
            {
                return new string[] { "Could not parse line " };
            }
        }
    }
}
