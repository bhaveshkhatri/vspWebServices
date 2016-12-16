using Microsoft.VisualStudio.TestTools.LoadTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Serialization;
using VspWS.Common;

namespace VspWS.Plugins.LoadTest
{
    public class LoadTestPostProcessor : ILoadTestPlugin
    {
        Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest loadTest;

        private LoadTestExecutionLedger ledger;

        [DefaultValue("")]
        [Description("Test-relative path to a folder where to output the JTL file.")]
        public string RelativePathToJtlFileFolder { get; set; }

        private string JtlFileName;

        private string JtlFilePath { get { return RelativePathToJtlFileFolder + JtlFileName; } }

        public void Initialize(Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest loadTest)
        {
            InitializeConcurrencyCapabilities();
            this.loadTest = loadTest;
            this.loadTest.TestStarting += new EventHandler<TestStartingEventArgs>(myLoadTest_LoadTestStarting);
            this.loadTest.LoadTestFinished += new EventHandler(myLoadTest_LoadTestFinished);

            JtlFileName = string.Format(Constants.JtlFileNameFormat, this.loadTest.Name);
            ledger = new LoadTestExecutionLedger();
            this.loadTest.Context.Add(Constants.LedgerKey, ledger);
        }

        private static void InitializeConcurrencyCapabilities()
        {
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = Constants.ConcurrencyLevel;
            var maxThreads = Constants.ConcurrencyLevel;
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }

        private void myLoadTest_LoadTestStarting(object sender, TestStartingEventArgs e)
        {
            //TODO: Initialize web test ledger with load test scenario specific information
        }

        void myLoadTest_LoadTestFinished(object sender, EventArgs e)
        {
            var executionLedgers = GetExecutionLedgers(ledger);

            ValidateDurationsAgainstTimeouts(executionLedgers);

            OutputToJtl(executionLedgers);
        }

        private IEnumerable<WebTestExecutionLedger> GetExecutionLedgers(LoadTestExecutionLedger ledger)
        {
            return ledger.WebTestExecutionLedgers.Values;
        }

        private void ValidateDurationsAgainstTimeouts(IEnumerable<WebTestExecutionLedger> executionLedgers)
        {
            foreach (var executionLedger in executionLedgers)
            {
                var averageDuration = executionLedger.WebRequestExecutionLedgers.Select(x => x.Value.Duration(executionLedger.MeasurementType)).Average();
                var averageProcessStarted = executionLedger.WebRequestExecutionLedgers.Where(x => x.Value.ProcessStarted.HasValue).Select(x => x.Value.ProcessStarted.Value).Average();
                var averageProcessCompleted = executionLedger.WebRequestExecutionLedgers.Where(x => x.Value.ProcessCompleted.HasValue).Select(x => x.Value.ProcessCompleted.Value).Average();
                var averageRequestStarted = executionLedger.WebRequestExecutionLedgers.Where(x => x.Value.RequestStarted.HasValue).Select(x => x.Value.RequestStarted.Value).Average();
                var averageRequestCompleted = executionLedger.WebRequestExecutionLedgers.Where(x => x.Value.RequestCompleted.HasValue).Select(x => x.Value.RequestCompleted.Value).Average();
                var exceededMaximumAverageDuration = executionLedger.MaximumAverageDurationInMilliseconds > 0 && averageDuration > executionLedger.MaximumAverageDurationInMilliseconds;
                var now = Utils.Now();
                executionLedger.AppendedRequestExecutionLedgers.TryAdd(Guid.NewGuid(), new WebRequestExecutionLedger
                {
                    IsSuccess = !exceededMaximumAverageDuration,
                    ProcessStarted = averageProcessStarted,
                    ProcessCompleted = averageProcessCompleted,
                    RequestStarted =  averageRequestStarted,
                    RequestCompleted = averageRequestCompleted,
                    ResponseCode = exceededMaximumAverageDuration ? HttpStatusCode.Ambiguous : HttpStatusCode.OK,
                    LabelSuffix = Constants.AverageLabelSuffix,
                    AdditionalInformation = exceededMaximumAverageDuration ? Constants.Messages.ExceededMaximumAverageDuration : string.Empty
                });
            }
        }

        private void OutputToJtl(IEnumerable<WebTestExecutionLedger> executionLedgers)
        {
            if (!string.IsNullOrWhiteSpace(RelativePathToJtlFileFolder))
            {
                Directory.CreateDirectory(RelativePathToJtlFileFolder);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(TestResults));
            TextWriter writer = new StreamWriter(JtlFilePath, false);
            XmlSerializerNamespaces serializerNamespace = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            serializerNamespace.Add("", "");

            serializer.Serialize(writer, MapToTestResults(executionLedgers), serializerNamespace);
        }

        private TestResults MapToTestResults(IEnumerable<WebTestExecutionLedger> executionLedgers)
        {
            var testResults = new TestResults();
            foreach(var executionLedger in executionLedgers)
            {
                testResults.HttpSamples.AddRange(BuildHttpSamples(executionLedger, executionLedger.WebRequestExecutionLedgers.Values));
                testResults.HttpSamples.AddRange(BuildHttpSamples(executionLedger, executionLedger.AppendedRequestExecutionLedgers.Values));
            }

            return testResults;
        }

        private List<HttpSample> BuildHttpSamples(WebTestExecutionLedger executionLedger, ICollection<WebRequestExecutionLedger> values)
        {
            return values
                .Select(x => new HttpSample
                {
                    ElapsedTimeInMilliseconds = (int)Math.Round(x.Duration(executionLedger.MeasurementType), 0),
                    ResponseCode = (int)x.ResponseCode,
                    IsSuccess = x.IsSuccess,
                    Label = executionLedger.WebTestName + x.LabelSuffix,
                    MessageId = x.MessageId.ToString(),
                    MillisecondsSince19700101 = (long)(x.RequestStarted.Value - new DateTime(1970, 1, 1)).TotalMilliseconds,
                    AdditionalInformation = x.AdditionalInformation,
                    Hl7MessageVersion = x.Payloads.Any() ? x.Payloads[0].Hl7Message.Version : "",
                    Hl7PatientBirthYear = x.Payloads.Any() ? x.Payloads[0].Hl7Message.Message.PID.DateTimeOfBirth.Time.Year.ToString() : ""
                })
                .ToList();
        }
    }
}
