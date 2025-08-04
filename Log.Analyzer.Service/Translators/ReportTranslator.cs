using Log.Analyzer.ElasticSearch;
using System.Net;
using System.Text;

namespace Log.Analyzer.Service.Translators
{
    public static class ReportTranslator
    {
        public static string ToHTMLReport(this string data)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html><body>");
            sb.AppendLine("<h2>Daily Failure Analysis Report</h2>");
            sb.AppendLine(data);
            sb.AppendLine("</body></html>");

            string emailBody = sb.ToString();

            return emailBody;
        }

        public static string GenerateApplicationHtmlTable(this string application, string exception, string failure)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<h3> Application : {application}</h3>");

            if (!string.IsNullOrEmpty(exception))
            {
                sb.AppendLine(exception);
                sb.AppendLine("<br/>");
            }

            if (!string.IsNullOrEmpty(failure))
            {
                sb.AppendLine(failure);
                sb.AppendLine("<br/>");
            }

            string emailBody = sb.ToString();

            return emailBody;
        }

        public static string GenerateExceptionHtmlTable(List<LogData> failures, int? newFailureCount, int? existingFailureCount)
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetCSS());

            sb.AppendLine($"<h4>New exception count :  {newFailureCount} Existing exception count : {existingFailureCount}</h4>");
            sb.AppendLine($"<h4>Unique exception count : {failures.Count}</h4>");
            sb.Append("<table class='minimalistBlack'>");
            var TableFormat = "<thead><tr>" +
                                "<th>CID</th>" +                  
                                "<th>Msg</th>" +
                                "<th>ExceptionType</th>" +
                                "<th>Source</th>" +
                                "</tr></thead>";

            sb.Append(TableFormat);
            sb.Append("<tbody>");

            foreach (var failure in failures)
            {
                if (!string.IsNullOrEmpty(failure.Cid) || !string.IsNullOrWhiteSpace(failure.Msg))
                {
                    sb.Append($"<tr>" +
                    $"<td>{failure.Cid}</td>" +
                    $"<td>{failure.Msg}</td>" +
                    $"<td>{failure.ExceptionType}</td>" +
                     $"<td>{failure.Source}</td>" +
                    $"</tr>");
                }
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        public static string GenerateFailuresHtmlTable(List<LogData> failures, int? newFailureCount, int? existingFailureCount)
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetCSS());

            sb.AppendLine($"<h4>New failures count :  {newFailureCount} Existing failure count : {existingFailureCount}</h4>");
            sb.AppendLine($"<h4>Unique failure count : {failures.Count}</h4>");
            sb.Append("<table class='minimalistBlack'>");
            var TableFormat = "<thead><tr>" +
                                "<th>CID</th>" +
                                "<th>Api</th>" +
                                "<th>Verb</th>" +
                                "<th>Msg</th>" +
                                "</tr></thead>";

            sb.Append(TableFormat);
            sb.Append("<tbody>");

            foreach (var failure in failures)
            {
                if (!string.IsNullOrEmpty(failure.Cid) || !string.IsNullOrWhiteSpace(failure.Msg))
                {
                    sb.Append($"<tr>" +
                    $"<td>{failure.Cid}</td>" +
                    $"<td>{failure.Api}</td>" +
                    $"<td>{failure.Verb}</td>" +
                    $"<td>{failure.Msg}</td>" +
                    $"</tr>");
                }
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        public static string GetCSS()
        {
            return @"<head>
            <style type='text/css'>
              table.minimalistBlack {
                border: 1px solid #000000;
                border-collapse: collapse;
                text-align: left;
                width: 100%;
                font-family: Arial, sans-serif;
              }

              table.minimalistBlack th,
              table.minimalistBlack td {
                border: 1px solid #000000;
                padding: 8px 6px;
              }

              table.minimalistBlack tbody td {
                font-size: 13px;
                color: #333333;
              }

              table.minimalistBlack thead {
                background: #CFCFCF;
                background: -moz-linear-gradient(top, #dbdbdb 0%, #d3d3d3 66%, #CFCFCF 100%);
                background: -webkit-linear-gradient(top, #dbdbdb 0%, #d3d3d3 66%, #CFCFCF 100%);
                background: linear-gradient(to bottom, #dbdbdb 0%, #d3d3d3 66%, #CFCFCF 100%);
                border-bottom: 2px solid #000000;
              }

              table.minimalistBlack thead th {
                font-size: 14px;
                font-weight: bold;
                color: #000000;
                text-align: left;
              }

              table.minimalistBlack tfoot {
                font-size: 13px;
                font-weight: bold;
                color: #000000;
                border-top: 2px solid #000000;
              }

              table.minimalistBlack tfoot td {
                font-size: 18px;
              }

              .ghostBooking {
                color: red;
                font-weight: bold;
              }
            </style>
            </head>";
        }
    }
}