﻿using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public class VOL_SRK : Expression
    {
        ThermodynamicSystem _system;

        private List<Expression> _parameters = new List<Expression>();

        Variable _T;
        Variable _p;

        Expression _A;
        Expression _B;
        PhaseState Phase;
        Variable _R = new Variable("R", 8.3144621, SI.J / SI.mol / SI.K);
        public List<Expression> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }

        public VOL_SRK(Variable T, Variable p, Expression A, Expression B, PhaseState phase)
        {
            this._T = T;
            this._p = p;
            this._A = A;
            this._B = B;
            this.Phase = phase;

            Parameters.Add(T);
            Parameters.Add(p);
            // Parameters.Add(A);
            // Parameters.Add(B);

            //DiffFunctional = (cache, v) => idealVolume.Diff(cache, v);
            DiffFunctional = (cache, v) => NumDiff(cache, v);
            EvalFunctional = (cache) => Evaluate(cache);

        }

        public VOL_SRK(ThermodynamicSystem system, Variable T, Variable p, List<Variable> y)
        {
            _system = system;
            this._T = T;
            this._p = p;
            this.Phase = PhaseState.Vapour;
            Symbol = "VOL_SRK";
            var NC = system.Components.Count;
            var ai = new double[NC];
            var bi = new double[NC];
            var Ai = new Expression[NC];
            var aij = new Expression[NC, NC];
            var bij = new double[NC, NC];

            double[,] kij = new double[NC, NC];
            double[,] kbij = new double[NC, NC];


            //  Parameters.Add(T);
            //  Parameters.Add(p);
            // foreach (var c in y)
            //    Parameters.Add(c);



            var parameterSet = _system.BinaryParameters.FirstOrDefault(ps => ps.Name == "SRK");
            if (parameterSet != null)
            {
                kij = parameterSet.Matrices["kij"];
                if (parameterSet.Matrices.ContainsKey("kbij"))
                    kbij = parameterSet.Matrices["kbij"];
            }

            for (int i = 0; i < system.Components.Count; i++)
            {
                var comp = system.Components[i];
                var TC = comp.GetConstant(ConstantProperties.CriticalTemperature);
                var PC = comp.GetConstant(ConstantProperties.CriticalPressure);
                var AC = comp.GetConstant(ConstantProperties.AcentricFactor);

                if (comp.HasParameter(MethodTypes.RKS, "A"))
                    ai[i] = comp.GetParameter(MethodTypes.RKS, "A").ValueInSI;
                else
                    ai[i] = 0.42748 * Math.Pow(_R.ValueInSI, 2.0) * Math.Pow(TC.ValueInSI, 2.0) / PC.ValueInSI;

                if (comp.HasParameter(MethodTypes.RKS, "B"))
                    bi[i] = comp.GetParameter(MethodTypes.RKS, "B").ValueInSI;
                else
                    bi[i] = 0.0867 * _R.ValueInSI * TC.ValueInSI / PC.ValueInSI;

                var mi = 0.48 + 1.574 * AC.ValueInSI - 0.176 * Math.Pow(AC.ValueInSI, 2);
                var TR = Sym.Binding("TR", T / TC);
                var alphai = Sym.Pow(1.0 + mi * (1 - Sym.Sqrt(TR)), 2.0);
                Ai[i] = alphai * ai[i];
            }

            for (int i = 0; i < system.Components.Count; i++)
            {
                for (int j = 0; j < system.Components.Count; j++)
                {
                    aij[i, j] = Sym.Sqrt(Ai[i] * Ai[j]) * (1 - kij[i, j]);
                    bij[i, j] = (bi[i] + bi[j]) / 2.0 * (1 - kbij[i, j]);
                }
            }

            _A = Sym.Sum(0, NC, i => y[i] * Sym.Sum(0, NC, j => y[j] * aij[i, j]));
            _B = Sym.Sum(0, NC, i => y[i] * Sym.Sum(0, NC, j => y[j] * bij[i, j]));

            DiffFunctional = (cache, v) => NumDiff(cache, v);
            EvalFunctional = (cache) => Evaluate(cache);
        }

        double NumDiff(Evaluator cache, Variable v)
        {
            var h = 1e-8;
            double original = v.ValueInSI;
            double vnull = Evaluate(cache);
            v.ValueInSI = original + h;
            double vplus = Evaluate(new Evaluator());
            // v.ValueInSI = original - h;
            // double vminus = Evaluate(new Evaluator());
            v.ValueInSI = original;
            var dxdv = (vplus - vnull) / (1 * h);
            return dxdv;
        }

        double Evaluate(Evaluator cache)
        {
            var T = _T.ValueInSI;
            var P = _p.ValueInSI;
            var A = _A.Eval(cache);
            var B = _B.Eval(cache);
            var R = _R.ValueInSI;

            var PSTR = -(P * Math.Pow(B, 2.0) + R * T * B - A) / 3.0 / P - Math.Pow(R * T / 3.0 / P, 2.0);
            var QSTR = -Math.Pow(R * T / 3.0 / P, 3.0) - R * T * (P * Math.Pow(B, 2.0) + R * T * B - A) / 6.0 / Math.Pow(P, 2.0) - A * B / 2.0 / P;
            var DISKR = Math.Pow(QSTR, 2.0) + Math.Pow(PSTR, 3.0);
            var VV = 1e-3;
            var VL = 1e-4;

            var rhoc = 0.2599 / B;


            if (DISKR < 0)
            {
                var RSTR = Math.Sign(QSTR) * Math.Sqrt(Math.Abs(PSTR));
                var COSPHI = QSTR / Math.Pow(RSTR, 3.0);
                var PHI = Math.Acos(COSPHI);
                var X1 = -2.0 * RSTR * Math.Cos(PHI / 3.0);
                var X2 = 2.0 * RSTR * Math.Cos((Math.PI - PHI) / 3.0);
                var X3 = 2.0 * RSTR * Math.Cos((Math.PI + PHI) / 3.0);
                VV = Math.Max(X1, Math.Max(X2, X3)) + R * T / 3.0 / P;
                VL = Math.Min(X1, Math.Min(X2, X3)) + R * T / 3.0 / P;
            }
            else
            {

                var H1 = -QSTR + Math.Sqrt(DISKR);
                var H2 = -QSTR - Math.Sqrt(DISKR);

                if (Double.IsNaN(H1))
                    H1 = 0;
                if (Double.IsNaN(H2))
                    H2 = 0;

                var H3 = Math.Sign(H1);
                var H4 = Math.Sign(H2);
                var V = H3 * Math.Pow(Math.Abs(H1), 1.0 / 3.0) + H4 * Math.Pow(Math.Abs(H2), 1.0 / 3.0);
                VL = V + R * T / 3.0 / P;
                VV = V + R * T / 3.0 / P;

                //HACK: Very simple extrapolation of densities in non-physical region. Research needed on my side. Refer: Boston Mathias, Gundersen, Prausnitz
                if (VL > 1.0 / rhoc)
                    VL = 1.0 / rhoc;
                if (VV < 1.0 / rhoc)
                    VV = 1.0 / rhoc;
            }

            if (Phase == PhaseState.Vapour)
                return VV;
            else
                return VL;
        }
        //public override HashSet<Variable> Incidence()
        //{
        //    var inc = new HashSet<Variable>();

        //    foreach (var parameter in Parameters)
        //    {
        //        inc.UnionWith(parameter.Incidence());
        //    }
        //    return inc;
        //}
    }

    public class K_EOS_SRK : Expression
    {
        ThermodynamicSystem _system;
        int index = -1;
        int NC;

        Variable T;
        Variable p;
        List<Variable> x;
        List<Variable> y;

        Expression _kEOS_SRK;
        Expression _dphiDt;
        Expression _dphiDp;
        Expression[] _dphiDx;
        Expression[] _dphiDy;
        Expression[] PHI = new Expression[2];

        private List<Expression> _parameters = new List<Expression>();
        Variable R = new Variable("R", 8.3144621, SI.J / SI.mol / SI.K);

        public List<Expression> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }

        public K_EOS_SRK(ThermodynamicSystem system, Variable T, Variable p, List<Variable> x, List<Variable> y, int idx)
        {
            index = idx;
            _system = system;

            this.T = T;
            this.p = p;
            this.x = x;
            this.y = y;
            Symbol = "EQ_SRK";

            Parameters.Add(T);
            Parameters.Add(p);
            foreach (var c in x)
                Parameters.Add(c);
            foreach (var c in y)
                Parameters.Add(c);

            NC = system.Components.Count;


            var ai = new double[NC];
            var bi = new double[NC];
            var Ai = new Expression[NC];
            var aij = new Expression[NC, NC];
            var bij = new double[NC, NC];

            double[,] kij = new double[NC, NC];
            double[,] kbij = new double[NC, NC];

            var parameterSet = _system.BinaryParameters.FirstOrDefault(ps => ps.Name == "SRK");
            if (parameterSet != null)
            {
                kij = parameterSet.Matrices["kij"];

                if (parameterSet.Matrices.ContainsKey("kbij"))
                    kbij = parameterSet.Matrices["kbij"];
            }

            for (int i = 0; i < system.Components.Count; i++)
            {
                var comp = system.Components[i];
                var TC = comp.GetConstant(ConstantProperties.CriticalTemperature);
                var PC = comp.GetConstant(ConstantProperties.CriticalPressure);
                var AC = comp.GetConstant(ConstantProperties.AcentricFactor);

                if (comp.HasParameter(MethodTypes.RKS, "A"))
                    ai[i] = comp.GetParameter(MethodTypes.RKS, "A").ValueInSI;
                else
                    ai[i] = 0.42748 * Math.Pow(R.ValueInSI, 2.0) * Math.Pow(TC.ValueInSI, 2.0) / PC.ValueInSI;

                if (comp.HasParameter(MethodTypes.RKS, "B"))
                    bi[i] = comp.GetParameter(MethodTypes.RKS, "B").ValueInSI;
                else
                    bi[i] = 0.0867 * R.ValueInSI * TC.ValueInSI / PC.ValueInSI;

                var mi = 0.48 + 1.574 * AC.ValueInSI - 0.176 * Math.Pow(AC.ValueInSI, 2);
                var TR = Sym.Binding("TR", T / TC);
                var alphai = Sym.Pow(1.0 + mi * (1 - Sym.Sqrt(TR)), 2.0);
                Ai[i] = alphai * ai[i];
            }


            for (int i = 0; i < system.Components.Count; i++)
            {
                for (int j = 0; j < system.Components.Count; j++)
                {
                    aij[i, j] = Sym.Sqrt(Ai[i] * Ai[j]) * (1 - kij[i, j]);
                    bij[i, j] = (bi[i] + bi[j]) / 2.0 * (1 - kbij[i, j]);
                }
            }


            for (int ph = 0; ph < 2; ph++)
            {
                var z = x;
                if (ph == 1)
                    z = y;

                var am = Sym.Sum(0, NC, i => z[i] * Sym.Sum(0, NC, j => z[j] * aij[i, j]));
                var asi = Sym.Sum(0, NC, j => z[j] * aij[idx, j]);
                var bm = Sym.Sum(0, NC, i => z[i] * Sym.Sum(0, NC, j => z[j] * bij[i, j]));
                var bsi = Sym.Sum(0, NC, j => z[j] * bij[idx, j]);
                var vme = new VOL_SRK(T, p, am, bm, ph == 0 ? PhaseState.Liquid : PhaseState.Vapour);
                var vm = Sym.Binding("vm", vme);
                var Bi = 2 * bsi - bm;
                var zm = p * vm / (R * T);

                var eterm = -am / (bm * R * T) * (2 * asi / am - Bi / bm) * Sym.Ln(1 + bm / vm) + Bi / bm * (zm - 1);
                var eVariable = Sym.Binding("RKS_E", eterm);

                //PHI[ph] = (R * T) / ((vm - bm) * p) * Sym.Exp(eVariable);
                PHI[ph] = 1.0 / ((vm - bm)) * Sym.Exp(eVariable);

            }
            //_kEOS_SRK = Sym.Exp(lnPHI[0] - lnPHI[1]);
            _kEOS_SRK = PHI[0] / PHI[1];
            _dphiDx = new Expression[NC];
            _dphiDy = new Expression[NC];

            DiffFunctional = (cache, v) => _kEOS_SRK.Diff(cache, v);
            EvalFunctional = (cache) => Evaluate(cache);
        }

        double Evaluate(Evaluator cache)
        {
            //var phiL = Sym.Exp(lnPHI[0]).Eval(cache);
            // var phiV = Sym.Exp(lnPHI[1]).Eval(cache);
            var value = _kEOS_SRK.Eval(cache);
            //var value = phiL / phiV;
            return value;
        }
        public override Expression SymbolicDiff(Variable var)
        {
            if (T == var)
            {
                if (_dphiDt == null)
                    _dphiDt = _kEOS_SRK.SymbolicDiff(var);
                return _dphiDt;
            }
            if (p == var)
            {
                if (_dphiDp == null)
                    _dphiDp = _kEOS_SRK.SymbolicDiff(var);
                return _dphiDp;
            }

            for (int i = 0; i < NC; i++)
            {
                if (x[i] == var)
                {
                    if (_dphiDx[i] == null)
                        _dphiDx[i] = _kEOS_SRK.SymbolicDiff(var);
                    return _dphiDx[i];
                }
                if (y[i] == var)
                {
                    if (_dphiDy[i] == null)
                        _dphiDy[i] = _kEOS_SRK.SymbolicDiff(var);
                    return _dphiDy[i];
                }
            }
            return new IntegerLiteral(0);
        }


        public override HashSet<Variable> Incidence()
        {
            var inc = new HashSet<Variable>();

            foreach (var parameter in Parameters)
            {
                inc.UnionWith(parameter.Incidence());
            }
            return inc;
        }

        public override string ToString()
        {
            return "K_EOS_SRK(T,p,x,y)";
        }
    }
}
