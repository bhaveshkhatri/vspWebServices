using Microsoft.VisualStudio.TestTools.LoadTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VspWS.Common;
using VspWS.DataAccess;

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
            this.loadTest = loadTest;
            this.loadTest.LoadTestFinished += new EventHandler(myLoadTest_LoadTestFinished);

            JtlFileName = string.Format("{0}-VSPT.jtl", this.loadTest.Name);
            ledger = new LoadTestExecutionLedger();
            this.loadTest.Context.Add(Constants.LedgerKey, ledger);
        }

        void myLoadTest_LoadTestFinished(object sender, EventArgs e)
        {
            var executionLedgers = GetExecutionLedgers(ledger);

            PopulateDurationsFromDatabaseAsNeeded(executionLedgers);

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
                    LabelSuffix = "-Average",
                    AdditionalInformation = exceededMaximumAverageDuration ? "Average duration exceeded threshold." : ""
                });

                foreach (var requestLedger in executionLedger.WebRequestExecutionLedgers.Values)
                {
                    if (executionLedger.MaximumSingleDurationInMilliseconds > 0
                        && requestLedger.Duration(executionLedger.MeasurementType) > executionLedger.MaximumSingleDurationInMilliseconds)
                    {
                        requestLedger.IsSuccess = false;
                        requestLedger.AdditionalInformation = "Request duration exceeded threshold.";
                    }
                }
            }
        }

        private void PopulateDurationsFromDatabaseAsNeeded(IEnumerable<WebTestExecutionLedger> executionLedgers)
        {
            List<Task> tasks = new List<Task>();
            foreach(var executionLedger in executionLedgers.Where(x => x.MeasurementType == MeasurementType.ProcessingDuration || x.MeasurementType == MeasurementType.TotalDuration))
            {
                var requestLedgers = executionLedger.WebRequestExecutionLedgers.Values.Where(x => x.IsSuccess);
                foreach (var requestLedger in requestLedgers)
                {
                    tasks.Add(new Task(() =>
                    {
                        var isComplete = false;
                        var startTime = Utils.Now();
                        var endTime = startTime.AddMilliseconds(executionLedger.MaximumProcessingWaitTimeInMilliseconds).AddMilliseconds(-1* requestLedger.ProcessingResultsPollingIntervalInMilliseconds);
                        while (!isComplete && Utils.Now() <= endTime)
                        {
                            Thread.Sleep(requestLedger.ProcessingResultsPollingIntervalInMilliseconds);
                            using (var dal = new AlSysDAL(requestLedger.AlSysConnectionString))
                            {
                                var item = dal.GetEhrMessageTrackingInfo(requestLedger.MessageId);

                                if (item != null && item.ProcessCompletedOn != null)
                                {
                                    requestLedger.ProcessStarted = item.ProcessStartedOn;
                                    requestLedger.ProcessCompleted = item.ProcessCompletedOn;
                                    isComplete = true;
                                }
                            }
                        }

                        if(!(requestLedger.ProcessStarted.HasValue && requestLedger.ProcessCompleted.HasValue))
                        {
                            requestLedger.IsSuccess = false;
                            requestLedger.ResponseCode = HttpStatusCode.Ambiguous;
                            requestLedger.AdditionalInformation = "Processing duration could not be determined in time.";
                        }
                    }));
                }
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                //DO NOTHING.
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
                    AdditionalInformation = x.AdditionalInformation
                })
                .ToList();
        }

        private class WebTestKeyedWebRequestLedger
        {
            public string WebTestName { get; set; }

            public WebRequestExecutionLedger RequestLedger { get; set; }

            public WebTestExecutionLedger WebTestLedger { get; set; }
        }
    }
}
