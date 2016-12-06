using Microsoft.VisualStudio.TestTools.LoadTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VspWS.Plugins.LoadTest
{
    public class LoadTestPostProcessor : ILoadTestPlugin
    {
        Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest loadTest;

        private LoadTestExecutionLedger ledger;

        [DefaultValue("..\\PerformanceTestResults\\")]
        [Description("Test-relative path to a folder where to output the JTL file.")]
        public string RelativePathToJtlFileFolder { get; set; }

        private string JtlFileName;

        public void Initialize(Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest loadTest)
        {
            this.loadTest = loadTest;
            this.loadTest.LoadTestFinished += new EventHandler(myLoadTest_LoadTestFinished);

            JtlFileName = string.Format("{0}.jtl", this.loadTest.Name);
            ledger = new LoadTestExecutionLedger();
            this.loadTest.Context.Add(Constants.LedgerKey, ledger);
            //Debugger.Launch();
        }

        void myLoadTest_LoadTestFinished(object sender, EventArgs e)
        {
            // TODO
            var x = ledger;
        }
    }
}
