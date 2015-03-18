using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Keyoti.RapidFindReplace.WPF;
using Keyoti.RapidFindReplace.WPF.TextMatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Tests
{
    [TestClass]
    public class QueueTests
    {
        [TestMethod]
        public void TestRunQueue()
        {
            RunQueue rq = new RunQueue(5);
            FillRunQueue(rq, GetTestRuns());

            //now wrap
            rq.Enqueue(new Run("bbbb"));
            Assert.AreEqual(5, rq.Count);
            rq.Enqueue(new Run("cccc"));
            Assert.AreEqual(5, rq.Count);

            Assert.AreEqual("5555 6666", rq[0].Text);
            Assert.AreEqual("7777 8888", rq[1].Text);
            Assert.AreEqual("9999 aaaa", rq[2].Text);
            Assert.AreEqual("bbbb", rq[3].Text);
            Assert.AreEqual("cccc", rq[4].Text);

        }

        public static void FillRunQueue(RunQueue rq, List<Run> lr)
        {
            
            for (int i = 0; i < lr.Count; i++)
            {
                rq.Enqueue(lr[i]);
                Assert.AreEqual(i + 1, rq.Count);
                Assert.AreEqual(lr[i].Text, rq[i].Text);
            }
        }

        [TestMethod]
        public void TestScanRunQueue()
        {
            RunQueue rq = new RunQueue(5) ;
            
            FillRunQueue(rq, GetTestRuns());

            RunQueue rq_clone = rq.Clone();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            //easy one
            model.ScanQueuedRuns(rq, new Keyoti.RapidFindReplace.WPF.Query("1111"), 
                delegate(Run run, int index, int length){
                    Assert.AreEqual(rq_clone[0].Run, run);
                    Assert.AreEqual(0, index);
                    Assert.AreEqual(4, length);
                }
            );

            //easy one
            model.ScanQueuedRuns(rq, "4444",
                delegate(Run run, int index, int length)
                {
                    Assert.AreEqual(rq_clone[1].Run, run);
                    Assert.AreEqual(5, index);
                    Assert.AreEqual(4, length);
                }
            );

            //harder one
            int callInc = 0;
            model.ScanQueuedRuns(rq, "44445555",
                delegate(Run run, int index, int length)
                {
                    if (callInc == 0)
                    {
                        Assert.AreEqual(rq_clone[1].Run, run);
                        Assert.AreEqual(5, index);
                        Assert.AreEqual(4, length);
                    }
                    else if (callInc == 1)
                    {
                        Assert.AreEqual(rq_clone[2].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(4, length);
                    }
                    callInc++;
                }
            );

            //harder one
            callInc = 0;
            RunQueue rq2 = new RunQueue(5);
            
            FillRunQueue(rq2, GetTestRuns2());
            RunQueue rq2_clone = rq2.Clone();
            model.ScanQueuedRuns(rq2, "22223",
                delegate(Run run, int index, int length)
                {
                    if (callInc == 0)
                    {
                        Assert.AreEqual(rq2_clone[0].Run, run);
                        Assert.AreEqual(2, index);
                        Assert.AreEqual(4, length);
                    }
                    else if (callInc == 1)
                    {
                        Assert.AreEqual(rq2_clone[1].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(1, length);
                    }

                    Assert.IsTrue(callInc < 2);
                    callInc++;
                }
            );
        
        }

        [TestMethod]
        public void TestScanRunQueueMultipleTimes()
        {
            //harder one
            int callInc = 0;
            RunQueue rq2 = new RunQueue(5);
            FillRunQueue(rq2, GetTestRuns2());
            RunQueue rq2_clone = rq2.Clone();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.ScanQueuedRuns(rq2, "3",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc < 1);
                    if (callInc == 0)
                    {
                        Assert.AreEqual(rq2_clone[1].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(1, length);
                    }
                    callInc++;
                    
                }
            );
            //add new item and search again, this time we don't want to hit 3 again
            rq2.Enqueue(new Run("xxx"));
            model.ScanQueuedRuns(rq2, "3",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc++ < 0);//we dont want to hit this at all
                }
            );

        }

        [TestMethod]
        public void TestScanRunQueueMultipleTimes2()
        {
            //harder one
            int callInc = 0;
            RunQueue rq2 = new RunQueue(5);
            FillRunQueue(rq2, GetTestRuns2());
            RunQueue rq2_clone = rq2.Clone();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.ScanQueuedRuns(rq2, "3",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc < 1);
                    if (callInc == 0)
                    {
                        Assert.AreEqual(rq2_clone[1].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(1, length);
                    }
                    callInc++;

                }
            );
            //add new item and search again, this time we don't want to hit 3 again
            rq2.Enqueue(new Run("3"));
            model.ScanQueuedRuns(rq2, "3",
                delegate(Run run, int index, int length)
                {

                    Assert.AreEqual("3", run.Text);

                }
            );

        }

        [TestMethod]
        public void TestScanRunQueueMultipleTimes3()
        {
            //harder one
            int callInc = 0;
            RunQueue rq2 = new RunQueue(5);
            FillRunQueue(rq2, GetTestRuns2());
            
            rq2.Enqueue(new Run("3 3"));
            rq2.Enqueue(new Run("232"));
            RunQueue rq2_clone = rq2.Clone();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.ScanQueuedRuns(rq2, "3",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc< 4);
                    if (callInc == 0)
                    {

                        Assert.AreEqual(rq2_clone[1].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(1, length);
                    }
                    else if (callInc == 1)
                    {

                        Assert.AreEqual(rq2_clone[3].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(1, length);
                    }
                    else if (callInc == 2)
                    {

                        Assert.AreEqual(rq2_clone[3].Run, run);
                        Assert.AreEqual(2, index);
                        Assert.AreEqual(1, length);
                    }
                    else if (callInc == 3)
                    {

                        Assert.AreEqual(rq2_clone[4].Run, run);
                        Assert.AreEqual(1, index);
                        Assert.AreEqual(1, length);
                    }
                    callInc++;

                }
            );
            

        }

        [TestMethod]
        public void TestQueuedRun()
        {
            QueuedRun qr = new QueuedRun(new Run("1234"));
            qr.RegisterHit( 2);
            Assert.AreEqual("\n\n34", qr.HitAvailableText);
        }


        [TestMethod]
        public void TestCrossRunMatch()
        {
            RunQueue queue = new RunQueue(2);
            Run r1 = new Run("1234"), r2 = new Run("5678");
            int callInc=0;
            queue.Enqueue(r1);
            queue.Enqueue(r2);
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.ScanQueuedRuns(queue, "3456",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc < 2);
                    if (callInc == 0)
                    {

                        Assert.AreEqual(r1, run);
                        Assert.AreEqual(2, index);
                        Assert.AreEqual(2, length);
                    }
                    else if (callInc == 1)
                    {
                        Assert.AreEqual(r2, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(2, length);
                    }
                    callInc++;
                }
             );
            Assert.AreEqual(2, callInc);
        }


        [TestMethod]
        public void TestCrossRunMatch2()
        {
            RunQueue queue = new RunQueue(2);
            Run r1 = new Run("1234co"), r2 = new Run("co5678");
            int callInc = 0;
            queue.Enqueue(r1);
            queue.Enqueue(r2);
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.ScanQueuedRuns(queue, "coco",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc < 2);
                    if (callInc == 0)
                    {

                        Assert.AreEqual(r1, run);
                        Assert.AreEqual(4, index);
                        Assert.AreEqual(2, length);
                    }
                    else if (callInc == 1)
                    {
                        Assert.AreEqual(r2, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(2, length);
                    }
                    callInc++;
                }
             );
            Assert.AreEqual(2, callInc);
        }

        [TestMethod]
        public void TestiNET()
        {
            
        }

        private static List<Run> GetTestRuns()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("1111 2222"));
            runs.Add(new Run("3333 4444"));
            runs.Add(new Run("5555 6666"));
            runs.Add(new Run("7777 8888"));
            runs.Add(new Run("9999 aaaa"));
            return runs;
        }

        private static List<Run> GetTestRuns2()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("1 2222"));
            runs.Add(new Run("3 4"));
            runs.Add(new Run("55555 6666"));

            return runs;
        }
    }
}
