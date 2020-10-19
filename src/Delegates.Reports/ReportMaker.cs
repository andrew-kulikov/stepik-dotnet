using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delegates.Reports
{
    public class ReportItem
    {
        public ReportItem(string propertyTitle, string propertyValue)
        {
            PropertyTitle = propertyTitle;
            PropertyValue = propertyValue;
        }

        public string PropertyTitle { get; }
        public string PropertyValue { get; }
    }

    public class Report
    {
        public Report(string title, ICollection<ReportItem> items)
        {
            Title = title;
            Items = items;
        }

        public string Title { get; }
        public ICollection<ReportItem> Items { get; }
    }

    public interface IReportMaker<in TData, out TReport>
    {
        TReport BuildReport(string title, TData data);
    }

    public class MedianReportMaker: IReportMaker<ICollection<Measurement>, Report>
    {
        private readonly MedianStatisticCalculator _calc = new MedianStatisticCalculator();

        public Report BuildReport(string title, ICollection<Measurement> data)
        {
            var medianHumidity = _calc.Calculate(data.Select(item => item.Humidity).ToList());
            var medianTemperature = _calc.Calculate(data.Select(item => item.Temperature).ToList());

            var items = new List<ReportItem>
            {
                new ReportItem("Temperature", medianTemperature.ToString()),
                new ReportItem("Humidity", medianHumidity.ToString())
            };

            return new Report(title, items);
        }
    }

    public class MeanStdReportMaker : IReportMaker<ICollection<Measurement>, Report>
    {
        private readonly MeanStdStatisticCalculator _calc = new MeanStdStatisticCalculator();

        public Report BuildReport(string title, ICollection<Measurement> data)
        {
            var medianHumidity = _calc.Calculate(data.Select(item => item.Humidity).ToList());
            var medianTemperature = _calc.Calculate(data.Select(item => item.Temperature).ToList());

            var items = new List<ReportItem>
            {
                new ReportItem("Temperature", medianTemperature.ToString()),
                new ReportItem("Humidity", medianHumidity.ToString()),
            };

            return new Report(title, items);
        }
    }

    public interface IReportExporter<in TReport, out TExport>
    {
        TExport Export(TReport report);
    }

    public interface IStructuredReportExporter
    {
        void AddCaption(string caption);
        void AddListStart();
        void AddListItem(string key, string value);
        void AddListEnd();
	}

    public abstract class ReportExporter: IReportExporter<Report, string>, IStructuredReportExporter
	{
        protected readonly StringBuilder _result = new StringBuilder();
		public string Export(Report report)
        {
			AddCaption(report.Title);

			AddListStart();

            foreach (var item in report.Items)
            {
                AddListItem(item.PropertyTitle, item.PropertyValue);
            }
     
            AddListEnd();

            return _result.ToString();
		}

        public abstract void AddCaption(string caption);
        public abstract void AddListStart();
        public abstract void AddListItem(string valueType, string entry);
        public abstract void AddListEnd();
    }

    public class HtmlReportExporter : ReportExporter
    {
        public override void AddCaption(string caption)
        {
			_result.Append($"<h1>{caption}</h1>");
		}

        public override void AddListStart()
        {
            _result.Append("<ul>");
        }

        public override void AddListItem(string key, string value)
        {
            _result.Append($"<li><b>{key}</b>: {value}");
        }

        public override void AddListEnd()
        {
			_result.Append("</ul>");
		}
	}

    public class MarkdownReportExporter : ReportExporter
    {
        public override void AddCaption(string caption)
        {
            _result.Append($"## {caption}\n\n");
        }

        public override void AddListStart()
        { }

        public override void AddListItem(string key, string value)
        {
            _result.Append($" * **{key}**: {value}\n\n");
        }

        public override void AddListEnd()
        { }
    }

    public interface IStatisticCalculator<in TIn, out TOut>
    {
        TOut Calculate(TIn data);
    }

    public class MedianStatisticCalculator : IStatisticCalculator<ICollection<double>, double>
    {
        public double Calculate(ICollection<double> items)
        {
			var sortedItems = items.OrderBy(z => z).ToList();
            var n = sortedItems.Count;

			if (n % 2 == 0)
                return (sortedItems[n / 2] + sortedItems[n / 2 - 1]) / 2;

            return sortedItems[n / 2];
		}
    }

    public class MeanStdStatisticCalculator : IStatisticCalculator<ICollection<double>, MeanAndStd>
    {
        public MeanAndStd Calculate(ICollection<double> items)
        {
            var mean = items.Average();
            var std = Math.Sqrt(items.Select(z => Math.Pow(z - mean, 2)).Sum() / (items.Count - 1));

            return new MeanAndStd
            {
                Mean = mean,
                Std = std
            };
		}
    }



	public static class ReportMakerHelper
	{
		public static string MeanAndStdHtmlReport(IEnumerable<Measurement> data)
		{
            var report = new MeanStdReportMaker().BuildReport("Mean and Std", data.ToList());
            return new HtmlReportExporter().Export(report);
        }

		public static string MedianMarkdownReport(IEnumerable<Measurement> data)
		{
            var report = new MedianReportMaker().BuildReport("Median", data.ToList());
            return new MarkdownReportExporter().Export(report);
        }

		public static string MeanAndStdMarkdownReport(IEnumerable<Measurement> measurements)
		{
            var report = new MeanStdReportMaker().BuildReport("Mean and Std", measurements.ToList());
            return new MarkdownReportExporter().Export(report);
        }

		public static string MedianHtmlReport(IEnumerable<Measurement> measurements)
		{
            var report = new MedianReportMaker().BuildReport("Median", measurements.ToList());
            return new HtmlReportExporter().Export(report);
        }
	}
}
