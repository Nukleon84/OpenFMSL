using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Evaluator
    {
        Dictionary<Expression, double> _expressionCache = new Dictionary<Expression, double>();
        Dictionary<Expression, Dictionary<Expression, double>> _cache = new Dictionary<Expression, Dictionary<Expression, double>>();

        public Dictionary<Expression, Dictionary<Expression, double>> DiffCache
        {
            get
            {
                return _cache;
            }

            set
            {
                _cache = value;
            }
        }

        public Dictionary<Expression, double> ExpressionCache
        {
            get
            {
                return _expressionCache;
            }
        }

        public void Reset()
        {
            ExpressionCache.Clear();
            DiffCache.Clear();
        }
    }

   
}
