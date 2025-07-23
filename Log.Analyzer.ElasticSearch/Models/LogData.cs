using Nest;

namespace Log.Analyzer.ElasticSearch
{
    public class LogData
    {
        [Text(Name = "type")]
        public string Type { get; set; }

        [Text(Name = "level")]
        public string Level { get; set; }

        [Text(Name = "exception_type")]
        public string ExceptionType { get; set; }

        [Text(Name = "stack_trace")]
        public string StackTrace { get; set; }

        [Text(Name = "cid")]
        public string Cid { get; set; }

        [Text(Name = "tid")]
        public string Tid { get; set; }

        [Text(Name = "app_name")]
        public string AppName { get; set; }

        [Text(Name = "verb")]
        public string Verb { get; set; }

        [Text(Name = "api")]
        public string Api { get; set; }

        [Text(Name = "msg")]
        public string Msg { get; set; }

        [Text(Name = "json_rq_headers.cnx-tenantid")]
        public string CnxTenantId { get; set; }

        [Text(Name = "request")]
        public string Request { get; set; }

        [Text(Name = "response")]
        public string Response { get; set; }

        [Text(Name = "status")]
        public string Status { get; set; }

        [Text(Name = "method_name")]
        public String MethodName { get; set; }

        [Text(Name = "log_time")]
        public DateTime TimeStamp { get; set; }

        [Text(Name = "time_taken_ms")]
        public decimal TimeTakenMs { get; set; }

        [Text(Name = "superpnr")]
        public string SuperPNR { get; set; }

        [Text(Name = "clientId")]
        public string ClientId { get; set; }

        [Text(Name = "client_id")]
        public string Client_Id { get; set; }

        [Text(Name = "userid")]
        public string UserId { get; set; }

        [Text(Name = "userid")]
        public string User_Id { get; set; }

        [Text(Name = "program_id")]
        public string Program_Id { get; set; }

        [Text(Name = "programId")]
        public string ProgramId { get; set; }

        [Text(Name = "json_rq_headers")]
        public Dictionary<string, string> RqHeaders { get; set; }

        [Text(Name = "super_pnr")]
        public string Super_pnr { get; set; }

        [Text(Name = "error_info")]
        public string error_info { get; set; }
    }
}
