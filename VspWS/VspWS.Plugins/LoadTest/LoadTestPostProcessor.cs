using Microsoft.VisualStudio.TestTools.LoadTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VspWS.Plugins.LoadTest
{
    public class LoadTestPostProcessor : ILoadTestPlugin
    {
        Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest myLoadTest;

        public void Initialize(Microsoft.VisualStudio.TestTools.LoadTesting.LoadTest loadTest)
        {
            myLoadTest = loadTest;
            myLoadTest.TestFinished += new EventHandler<TestFinishedEventArgs>(myLoadTest_LoadTestFinished);
        }

        void myLoadTest_LoadTestFinished(object sender, TestFinishedEventArgs e)
        {
            // TODO
        }
    }
}
