using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Keyoti.RapidFindReplace.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Tests
{
    [TestClass]
    public class QueryHistoryTests
    {
        [TestMethod]
        public void TestHistoryQueue()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.QueryHistoryCapacity = 5;
            Assert.AreEqual(0, model.QueryHistory.Count);


            model.AddQueryToHistory("t1");
            Assert.AreEqual(1, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[]{"t1"}));

            model.AddQueryToHistory("t2");
            Assert.AreEqual(2, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t2", "t1" }));

            model.AddQueryToHistory("t3");
            Assert.AreEqual(3, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t3", "t2", "t1" }));

            model.AddQueryToHistory("t4");
            Assert.AreEqual(4, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t4", "t3", "t2", "t1" }));

            model.AddQueryToHistory("t5");
            Assert.AreEqual(5, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t5", "t4", "t3", "t2", "t1" }));

            model.AddQueryToHistory("t6");
            Assert.AreEqual(5, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] {  "t6", "t5", "t4", "t3", "t2" }));

            model.QueryHistoryCapacity = 3;
            Assert.AreEqual(3, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t6", "t5", "t4" }));

            model.AddQueryToHistory("t7");
            Assert.AreEqual(3, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t7", "t6", "t5" }));

            model.QueryHistoryCapacity = 6;
            Assert.AreEqual(3, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t7", "t6", "t5" }));

            model.AddQueryToHistory("t8");
            Assert.AreEqual(4, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t8", "t7", "t6", "t5" }));

            model.AddQueryToHistory("t9");
            Assert.AreEqual(5, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t9", "t8", "t7", "t6", "t5" }));

            model.AddQueryToHistory("t10");
            Assert.AreEqual(6, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t10", "t9", "t8", "t7", "t6", "t5" }));

            model.AddQueryToHistory("t11");
            Assert.AreEqual(6, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] { "t11", "t10", "t9", "t8", "t7", "t6"  }));
            
            
            model.QueryHistoryCapacity = 0;
            Assert.AreEqual(0, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] {  }));

            model.AddQueryToHistory("t11");
            Assert.AreEqual(0, model.QueryHistory.Count);
            Assert.IsTrue(CheckContents(model.QueryHistory.GetEnumerator(), new string[] {  }));
           

        }

        bool CheckContents(IEnumerator<string> enumerator, string[] requiredContent)
        {
            for (int i = 0; i < requiredContent.Length; i++)
            {
                enumerator.MoveNext();
                if (enumerator.Current != requiredContent[i]) return false;
                
            }
            return true;
        }

     
    }
}
