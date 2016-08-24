using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceListeners
{
    class Program
    {
        static void Main(string[] args)
        {
           //  Nuget EnterpriseLibrary.Logging

            //  Create a configuration object
            var config = new LoggingConfiguration();

            // Make sure the log directory exists
            InitLogDirectory();

            //  Program the logger to ONLY log if the 
            //  switch is turned on in the app.config
            //  Set LoggingEnabled to FALSE...and nothing happens
            CreateLogOffSwitch(config);

            // Programmatically build logging configuration
            //  -> To plain text log file
            ConfigureSimpleLogging(config);

            //  -> To an xml file (no root element)
            ConfigureXmlLogging(config);

            //  -> To the application event log
            ConfigureEventLogLogging(config);

            //  -> To a rolling flat file (max 15...recreated every minute)
            ConfigureRollingFlatFileLogging(config);

            //  Create the Writer object 
            //  all logging will be done using this class
            var log = new LogWriter(config);

            //  Create an entry to send to the logger.
            var simpleEntry = new LogEntry();
            simpleEntry.Message = "Testing";
            simpleEntry.Severity = TraceEventType.Error;

            //  by default, it only writes to the FIRST defined logsource listener
            //  However, you can list each category to write to 
            //  Here I'm saying ALL by explicitly listing all categories
            simpleEntry.Categories = new string[] { "Simple", "EventLog", "xml", "Rolling" };

            //  Write the event
            LogEvent(log, simpleEntry);
        }

        private static void InitLogDirectory()
        {
            if (!Directory.Exists(@"C:\Temp"))
            {
                Directory.CreateDirectory(@"C:\Temp");
            }
        }
        private static void LogEvent(LogWriter log, LogEntry entry)
        {
            if (log.IsLoggingEnabled())
            {
                log.Write(entry);
            }
        }

        private static void ConfigureSimpleLogging(LoggingConfiguration config)
        {
            // Create a briefformatter object to write the log out
            TextFormatter briefFormatter = new TextFormatter("Timestamp: {timestamp(local)}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}ActivityId: {property(ActivityId)}{newline}Severity: {severity}{newline}Title:{title}{newline}");

            // Create a txt file listener and point to the formatter
            var flatFileTraceListener
                = new FlatFileTraceListener(
                        @"C:\Temp\FlatFile.log",
                        "----------------------------------------",
                        "----------------------------------------",
                        briefFormatter);

            //  Tell the config to use this (Category = Simple)
            config.AddLogSource("Simple", SourceLevels.All, true)
                .AddTraceListener(flatFileTraceListener);
        }
        private static void ConfigureEventLogLogging(LoggingConfiguration config)
        {
            //  Create an event log Object 
            // This maps to the appropriate log and ensure it exists
            var eventLog
                = new EventLog("Application", ".", "Enterprise Library Logging Sample");

            //  Create the listener
            var eventLogTraceListener
                = new FormattedEventLogTraceListener(eventLog);

            //  Tell the config to use this (Category = EventLog)
            config.AddLogSource("EventLog", SourceLevels.All, true)
                .AddTraceListener(eventLogTraceListener);
        }
        private static void ConfigureRollingFlatFileLogging(LoggingConfiguration config)
        {
            //  Create a briefFormatter to format the log parts
            TextFormatter briefFormatter = new TextFormatter("Timestamp: {timestamp(local)}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}ActivityId: {property(ActivityId)}{newline}Severity: {severity}{newline}Title:{title}{newline}");

            //  Create a rolling file trace listener
            //  This one will only append to the current log for a minute
            //  For demo purposes (set it higher for reals...by day or by hour)
            var rollingFlatFileTraceListener
                = new RollingFlatFileTraceListener(@"C:\Temp\RollingFlatFile.log",
                            "----------------------------------------",
                            "----------------------------------------",
                            briefFormatter,
                            20,
                            "yyyy-MM-dd-HH-mm",
                            RollFileExistsBehavior.Increment,
                            RollInterval.Minute,
                            15);

            //  Tell the config to use this (Category = Rolling)
            config.AddLogSource("Rolling", SourceLevels.All, true)
                .AddTraceListener(rollingFlatFileTraceListener);
        }
        private static void ConfigureXmlLogging(LoggingConfiguration config)
        {
            //  Create a listener that formats events as XML
            XmlTraceListener xmllog
                = new XmlTraceListener(@"C:\temp\application log.xml");

            //  Set it to only listen to Error AND ABOVE
            xmllog.Filter = new EventTypeFilter(SourceLevels.Error);

            //  Tell the config to use this (Category = xml)
            config.AddLogSource("xml", SourceLevels.All, true).AddTraceListener(xmllog);
        }
        private static void CreateLogOffSwitch(LoggingConfiguration config)
        {
            //  Go out to the app.config and load LoggingEnabled Setting
            var IsLoggingOn = Properties.Settings.Default.LoggingEnabled;

            //  Add a filter to control IsLoggingEnabled Property
            //  This turns the ENTIRE logging mechanism off with one switch
            config.Filters.Add(new LogEnabledFilter("LogEnabled", IsLoggingOn));
        }

    }
}
