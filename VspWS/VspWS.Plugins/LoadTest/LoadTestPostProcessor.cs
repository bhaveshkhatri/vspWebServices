using Microsoft.VisualStudio.TestTools.LoadTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VspWS.Plugins.LoadTest
{
    public class LoadTestPostProcessor : ILoadTestPlugin
    {
        Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest myLoadTest;

        private LoadTestExecutionLedger myLedger;

        public void Initialize(Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest loadTest)
        {
            myLoadTest = loadTest;
            myLoadTest.LoadTestFinished += new EventHandler(myLoadTest_LoadTestFinished);

            myLedger = new LoadTestExecutionLedger();
            myLoadTest.Context.Add(Constants.LedgerKey, myLedger);
            Debugger.Launch();
        }

        void myLoadTest_LoadTestFinished(object sender, EventArgs e)
        {
            // TODO
            var x = myLedger;
        }
    }
}
