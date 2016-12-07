using Microsoft.VisualStudio.TestTools.LoadTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
            //Debugger.Launch();
        }

        void myLoadTest_LoadTestFinished(object sender, EventArgs e)
        {
            // TODO
            var requests = ledger
                .WebTestExecutionLedgers
                .SelectMany(x => x.Value.WebRequestExecutionLedgers.Select(y => new WebTestKeyedWebRequestLedger
                {
                    WebTest = x.Key,
                    RequestLedger = y.Value
                }));

            List<Task> tasks = new List<Task>();
            foreach(var request in requests.Where(x => x.RequestLedger.MeasurementType == Common.MeasurementType.ProcessingDuration || x.RequestLedger.MeasurementType == Common.MeasurementType.TotalDuration))
            {
                tasks.Add(new Task(() => {
                    var isComplete = request.RequestLedger.IsSuccess;
                    do
                    {
                        Thread.Sleep(request.RequestLedger.ProcessingResultsPollingIntervalInMilliseconds);
                        using(var dal = new AlSysDAL(request.RequestLedger.AlSysConnectionString))
                        {
                            var item = dal.GetEhrMessageTrackingInfo(request.RequestLedger.MessageId);

                            if (item.ProcessCompletedOn != null)
                            {
                                request.RequestLedger.ProcessStarted = item.ProcessStartedOn;
                                request.RequestLedger.ProcessCompleted = item.ProcessCompletedOn;
                                isComplete = true;
                            }
                        }
                    } while (!isComplete);

                    request.RequestLedger.IsSuccess = request.RequestLedger.ProcessStarted.HasValue && request.RequestLedger.ProcessCompleted.HasValue;
                }));
            }

            foreach(var task in tasks)
            {
                task.Start();
            }

            try {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                //DO NOTHING.
            }

            OutputToJtl(requests);
        }

        private void OutputToJtl(IEnumerable<WebTestKeyedWebRequestLedger> requests)
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

            var value = new TestResults
            {
                HttpSamples = requests
                                .Select(x => new HttpSample
                                {
                                    ElapsedTimeInMilliseconds = (int)Math.Round(x.RequestLedger.Duration, 0),
                                    ResponseCode = (int)x.RequestLedger.ResponseCode,
                                    ResponseMessage = x.RequestLedger.ResponseCode.ToString(),
                                    IsSuccess = x.RequestLedger.IsSuccess,
                                    Label = x.WebTest,
                                    MessageIdAsThreadName = x.RequestLedger.MessageId.ToString()
                                })
                                .ToList()
            };
            serializer.Serialize(writer, value, serializerNamespace);
        }

        private class WebTestKeyedWebRequestLedger
        {
            public string WebTest { get; set; }

            public WebRequestExecutionLedger RequestLedger { get; set; }
        }
    }
}
