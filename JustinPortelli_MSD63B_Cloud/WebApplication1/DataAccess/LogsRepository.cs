﻿using Google.Api;
using Google.Cloud.Diagnostics.AspNet;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.DataAccess
{
    public class LogsRepository
    {
        public void LogError(Exception e)
        {
            //for beta versions you have check a small checkbox called include pre-release


            string projectId = "justinportellimsd63b"; //your project id
            string serviceName = "HomeAssignmentCloud"; //your application name (which you can invent)
            string version = "1";//any version which you're working on at the moment (which you can invent)
            var exceptionLogger = GoogleExceptionLogger.Create(projectId, serviceName, version);

            exceptionLogger.Log(e);
        }

        public void WriteLogEntry(string message)
        {
            var logId = "PFC_HomeAssignment_logs"; //a log id which you can invent to identify/distinguish your logs
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName("justinportellimsd63b", logId);  //your project id
            LogEntry logEntry = new LogEntry
            {
                LogName = logName.ToString(),
                Severity = LogSeverity.Info, //try experimenting with this to see the other values which you can log
                TextPayload = $"{message}" //here you can add more info if you want.  Don't worry you don't need to add the date as it is added automatically
            };
            MonitoredResource resource = new MonitoredResource { Type = "global" }; //this is a category; i suggest you leave it as it is.
        
            client.WriteLogEntries(logName, resource, null, new[] { logEntry });

        }
    }
}