using Microsoft.AspNetCore.Mvc;

namespace SiREAlgorithm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        [HttpPost]
        public ActionResult<double> PredictRevenue([FromBody] RevenuePredictionData data)
        {
            double[] revenue = data.HistoricalRevenue;
            double[] timeSeries = data.TimeSeries;

            if (revenue.Length != timeSeries.Length)
            {
                return BadRequest("Historical Revenue and Time Series must have the same number of values.");
            }

            double slope = FitSiRE(revenue, timeSeries);
            double futureTime = timeSeries.Length + 1;
            double futureRevenue = PredictFutureRevenue(slope, futureTime);

            return futureRevenue;
        }

        private double FitSiRE(double[] revenue, double[] timeSeries)
        {
            int n = revenue.Length;
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += timeSeries[i];
                sumY += revenue[i];
                sumXY += timeSeries[i] * revenue[i];
                sumX2 += timeSeries[i] * timeSeries[i];
            }

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

            return slope;
        }

        private double PredictFutureRevenue(double slope, double futureTime)
        {
            double futureRevenue = slope * futureTime;

            return futureRevenue;
        }
    }

    public class RevenuePredictionData
    {
        public double[] HistoricalRevenue { get; set; }
        public double[] TimeSeries { get; set; }
    }
}
