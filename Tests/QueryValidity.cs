using System;
using Keyoti.RapidFindReplace.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class QueryValidity
    {
        [TestMethod]
        public void MakeInvalid()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.UseRegularExpressions = true;
            Query q = new Query(".*[");
            RunQueue rq = new RunQueue(1);
            rq.Enqueue(new System.Windows.Documents.Run("hello this is it"));
            model.ScanQueuedRuns(rq, q, null);
            Assert.IsFalse(q.Valid);
            Assert.AreEqual("Regular expression error, parsing \".*[\" - Unterminated [] set.", q.ReasonInvalid);

        }
    }
}
