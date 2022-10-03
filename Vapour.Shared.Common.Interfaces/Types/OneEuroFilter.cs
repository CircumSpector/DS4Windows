using System;

namespace Vapour.Shared.Common.Types
{
    public class OneEuroFilter
    {
        protected double Dcutoff;
        protected LowpassFilter DxFilt;

        protected bool FirstTime;
        protected LowpassFilter XFilt;

        public OneEuroFilter(double minCutoff, double beta)
        {
            FirstTime = true;
            MinCutoff = minCutoff;
            Beta = beta;

            XFilt = new LowpassFilter();
            DxFilt = new LowpassFilter();
            Dcutoff = 1;
        }

        public double MinCutoff { get; set; }

        public double Beta { get; set; }

        public double Filter(double x, double rate)
        {
            var dx = FirstTime ? 0 : (x - XFilt.Last()) * rate;
            if (FirstTime) FirstTime = false;

            var edx = DxFilt.Filter(dx, Alpha(rate, Dcutoff));
            var cutoff = MinCutoff + Beta * Math.Abs(edx);

            return XFilt.Filter(x, Alpha(rate, cutoff));
        }

        protected double Alpha(double rate, double cutoff)
        {
            var tau = 1.0 / (2 * Math.PI * cutoff);
            var te = 1.0 / rate;
            return 1.0 / (1.0 + tau / te);
        }
    }

    public class LowpassFilter
    {
        protected bool FirstTime;
        protected double HatXPrev;

        public LowpassFilter()
        {
            FirstTime = true;
        }

        public double Last()
        {
            return HatXPrev;
        }

        public double Filter(double x, double alpha)
        {
            double hatX = 0;
            if (FirstTime)
            {
                FirstTime = false;
                hatX = x;
            }
            else
            {
                hatX = alpha * x + (1 - alpha) * HatXPrev;
            }

            HatXPrev = hatX;

            return hatX;
        }
    }
}