﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class NetworkValidation {

        /// <param name="checkSuccess">Funcion takes 'expected', 'actual' returns bool wether or not actual can be considerd a success</param>
        public static ValidationResult Validate(Network network, IEnumerable<InputExpectedResult> inputResults, Func<double[], double[], bool> checkSuccess) {
            ValidationResult totalResult = null;
            foreach(var cur in inputResults) {
                double[] actual = network.GetInputResult(cur.Input);

                double sse = SumSquaredError(cur.Output, actual);
                bool success = checkSuccess(cur.Output, actual);

                var res = new ValidationResult(sse, success);

                totalResult += res;
            }

            return totalResult;
        }

        public static double SquaredError(double target, double actual) {
            var val = Math.Pow(target - actual, 2);
            return val;
        }

        public static double SumSquaredError(double[] target, double[] actual) {
            double sum = 0;
            for(int i = 0; i < target.Length; i++) {
                double curErr = SquaredError(target[i], actual[i]);
                sum += curErr;
            }

            return sum;
        }
    }

    public class ValidationResult {
        private List<double> SSEs;

        public double AvgSSE => SSEs.Count > 0 ? SSEs.Average() : -1;

        public int EntryCount => SSEs.Count;

        public int Successes {
            get; private set;
        }

        public double SuccessPercentage => 100 * Successes / EntryCount;

        private ValidationResult() {
            SSEs = new List<double>();
        }

        public ValidationResult(double sse, bool isSuccess) : this() {
            SSEs.Add(sse);

            if(isSuccess) {
                Successes++;
            }
        }

        public double[] GetSSEs() {
            return SSEs.ToArray();
        }

        public static ValidationResult operator +(ValidationResult v1, ValidationResult v2) {
            var retVal = new ValidationResult();

            if(v1 != null) {
                retVal.SSEs.AddRange(v1.SSEs);
                retVal.Successes += v1.Successes;
            }
            if(v2 != null) {
                retVal.SSEs.AddRange(v2.SSEs);
                retVal.Successes += v2.Successes;
            }

            return retVal;
        }

        public override string ToString() {
            return $"Avg SSE: {AvgSSE:N5} | {SuccessPercentage:N1}% Correct";
        }
    }
}