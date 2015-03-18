using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyoti.RapidFindReplace.WPF;
using Keyoti.RapidFindReplace.WPF.TextMatching;
using System.Windows.Documents;

namespace Tests
{
    /// <summary>
    /// Summary description for MatcherTests
    /// </summary>
    [TestClass]
    public class MatcherTests
    {
        public MatcherTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Match_Simple_NoOptions()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly            = false;
            model.FindOptions.IgnorePunctuationCharacters   = false;
            model.FindOptions.IgnoreWhitespaceCharacters    = false;
            model.FindOptions.MatchCase                     = false;
            model.FindOptions.MatchPrefix                   = false;
            model.FindOptions.MatchSuffix                   = false;
            model.FindOptions.UseWildcards                  = false;



            Assert.AreEqual(-1, model.GetNextMatch_FOR_TESTING("xxx", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING( "text", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING("tex", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING("te", 0, "this is some text").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("t", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING("t", 1, "this is some text").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("this", 0, "this is some text").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("this is some text", 0, "this is some text").Start);
        }

        [TestMethod]
        public void Match_Simple_CaseSensitive()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly            = false;
            model.FindOptions.IgnorePunctuationCharacters   = false;
            model.FindOptions.IgnoreWhitespaceCharacters    = false;
            model.FindOptions.MatchCase                     = true;
            model.FindOptions.MatchPrefix                   = false;
            model.FindOptions.MatchSuffix                   = false;
            model.FindOptions.UseWildcards                  = false;



            Assert.AreEqual(-1, model.GetNextMatch_FOR_TESTING("xxx", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING("text", 0, "this is some text").Start);
            Assert.AreEqual(-1, model.GetNextMatch_FOR_TESTING("Text", 0, "this is some text").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("this", 0, "this is some text").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("this is some text", 0, "this is some text").Start);

            Assert.AreEqual(-1, model.GetNextMatch_FOR_TESTING("Text", 0, "this is some TEXT").Start);
        }

        [TestMethod]
        public void Match_Blend_WholeWords_Prefix()
        {
            //should essentially match whole words only
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly            = true;
            model.FindOptions.IgnorePunctuationCharacters   = false;
            model.FindOptions.IgnoreWhitespaceCharacters    = false;
            model.FindOptions.MatchCase                     = false;
            model.FindOptions.MatchPrefix                   = true;
            model.FindOptions.MatchSuffix                   = false;
            model.FindOptions.UseWildcards                  = false;



            Assert.AreEqual(-1, model.GetNextMatch_FOR_TESTING("xxx", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING("text", 0, "this is some text").Start);
            Assert.AreEqual(13, model.GetNextMatch_FOR_TESTING("Text", 0, "this is some text").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("this", 0, "this is some text").Start);
            Assert.AreEqual(-1, model.GetNextMatch_FOR_TESTING("thi", 0, "this is some text").Start);
            Assert.AreEqual(8, model.GetNextMatch_FOR_TESTING("thi", 0, "this is thi thi").Start);
            Assert.AreEqual(12, model.GetNextMatch_FOR_TESTING("thi", 9, "this is thi thi").Start);
            Assert.AreEqual(0, model.GetNextMatch_FOR_TESTING("this is some text", 0, "this is some text").Start);

        }

        [TestMethod]
        public void Match_Blend_WholeWords_Wildcards()
        {
            //should essentially match whole words only
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = true;
            model.FindOptions.IgnorePunctuationCharacters = false;
            model.FindOptions.IgnoreWhitespaceCharacters = false;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = true;



            Hit match;
            string testText = "this was some text";

            match = model.GetNextMatch_FOR_TESTING("th*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th*a", 0, testText);
            Assert.AreEqual(-1, match.Start);
            Assert.AreEqual(0, match.Length);

        }

        [TestMethod]
        public void Match_Wildcards()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = false;
            model.FindOptions.IgnoreWhitespaceCharacters = false;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = true;

            Hit match;
            string testText = "this was some text";




            match = model.GetNextMatch_FOR_TESTING("th*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th*", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(2, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th*xt", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(18, match.Length);

            match = model.GetNextMatch_FOR_TESTING("t*xt", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(18, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("xt*a", 0, testText);
            Assert.AreEqual(-1, match.Start);
            Assert.AreEqual(0, match.Length);

            match = model.GetNextMatch_FOR_TESTING("*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("***", 0, testText);
            Assert.AreEqual(-1, match.Start);
            Assert.AreEqual(0, match.Length);

            match = model.GetNextMatch_FOR_TESTING("***was***", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);



        }

        static List<Run> GetTestRuns()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("1111 2222 1111"));
            runs.Add(new Run("3333 4444"));
            runs.Add(new Run("5555 6666"));
            runs.Add(new Run("7777 8888"));
            runs.Add(new Run("9999 aaaa"));
            return runs;
        }


        [TestMethod]
        public void Match_ReplaceAll()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("aa aa aa"));

            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer container = new Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer(runs);
            container.IterationStartIndex = 3;// runs[0].Text.Length - 1;
            model.Query = new Query("aa");
            model.FindText(container/*, new Query("33([0-9])3")*/);

            model.ReplaceAllMatches("bb");


            Assert.AreEqual("bb bb bb", runs[0].Text);
        }

        [TestMethod]
        public void Match_Regex_Replace()
        {

            List<Run> runs = GetTestRuns();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.UseRegularExpressions = true;
            Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer container = new Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer(runs);
            model.Query = new Query("33([0-9])3");
            model.FindText(container/*, new Query("33([0-9])3")*/);


            model.SelectNextMatch();
            Assert.AreEqual(runs[1], model.CurrentMatch.Run);
            model.ReplaceMatch("xx$1");

            Assert.AreEqual("xx3 4444", runs[1].Text);
        
            //also need to fix single line tb when highlight goes outside of box width
        }

        [TestMethod]
        public void Match_Wildcard_Replace()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("1aa1 2222 1aa1"));
            runs.Add(new Run("3333 4444"));
            runs.Add(new Run("5555 6666"));
            runs.Add(new Run("7777 8888"));
            runs.Add(new Run("9999 aaaa"));
            
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.UseWildcards = true;
            Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer container = new Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer(runs);
            model.Query = new Query("1*1");
            model.FindText(container/*, new Query("33([0-9])3")*/);


            model.SelectNextMatch();
            Assert.AreEqual("1aa1", model.CurrentMatch.Text);
            Assert.AreEqual(runs[0], model.CurrentMatch.Run);
            model.ReplaceMatch("abcdefg");

            Assert.AreEqual("abcdefg 2222 1aa1", runs[0].Text);

            
            Assert.AreEqual(runs[0], model.CurrentMatch.Run);
            model.ReplaceMatch("abcdefg");
            Assert.AreEqual("abcdefg 2222 abcdefg", runs[0].Text);

            
        }

        [TestMethod]
        public void Match_Wildcard_Replace2()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("TextBox search word"));
            runs.Add(new Run("TextBox"));
            runs.Add(new Run("TextBox"));
            runs.Add(new Run("Hello"));
            runs.Add(new Run("here is some more"));
            runs.Add(new Run("and some more"));

            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.UseWildcards = true;
            Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer container = new Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer(runs);
            model.Query = new Query("r*");
            model.FindText(container);


            model.SelectNextMatch();
            Assert.AreEqual("r", model.CurrentMatch.Text);
            Assert.AreEqual(runs[0], model.CurrentMatch.Run);
            //model.ReplaceMatch("   ");
            model.ReplaceAllMatches("   ");

            Assert.AreEqual("TextBox sea   ch wo   d", runs[0].Text);


           


        }

        [TestMethod]
        public void Match_Standard_Replace()
        {

            List<Run> runs = GetTestRuns();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer container = new Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer(runs);
            model.Query = new Query("1111");
            model.FindText(container/*, new Query("33([0-9])3")*/);


            model.SelectNextMatch();
            Assert.AreEqual(runs[0], model.CurrentMatch.Run);
            model.ReplaceMatch("1");

            Assert.AreEqual("1 2222 1111", runs[0].Text);

            model.ReplaceMatch("1");

            Assert.AreEqual("1 2222 1", runs[0].Text);

            //also need to fix single line tb when highlight goes outside of box width
        }


        [TestMethod]
        public void Match_Regex()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = false;
            model.FindOptions.IgnoreWhitespaceCharacters = false;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = false;
            model.FindOptions.UseRegularExpressions = true;

            Hit match;
            string testText = "this was some text";

            match = model.GetNextMatch_FOR_TESTING("th.*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th.*", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(18, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th.*xt", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(18, match.Length);

            match = model.GetNextMatch_FOR_TESTING("t.*xt", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(18, match.Length);

            match = model.GetNextMatch_FOR_TESTING("th.*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("xt.*a", 0, testText);
            Assert.AreEqual(-1, match.Start);
            Assert.AreEqual(0, match.Length);

            match = model.GetNextMatch_FOR_TESTING(".*as", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING(".*.*.*", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(18, match.Length);

            
            
            testText = "this was some text\r\nxxxx yyy";


            //Go with VS behaviour, which is !SingleLine mode (ie . cannot eat \n)
            match = model.GetNextMatch_FOR_TESTING("text.*yyy", 0, testText);
            Assert.AreEqual(-1, match.Start);
            Assert.AreEqual(0, match.Length);


            testText = "This was SOME text\r\nxxxx yyy";

            model.FindOptions.MatchCase = true;
            match = model.GetNextMatch_FOR_TESTING("th[a-z]s", 0, testText);
            Assert.AreEqual(-1, match.Start);
            Assert.AreEqual(0, match.Length);

            model.FindOptions.MatchCase = true;
            match = model.GetNextMatch_FOR_TESTING("Th[a-z]s", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(4, match.Length);

            model.FindOptions.MatchCase = true;
            match = model.GetNextMatch_FOR_TESTING("S.*E", 0, testText);
            Assert.AreEqual(9, match.Start);
            Assert.AreEqual(4, match.Length);

            



        }


        [TestMethod]
        public void Match_Regex_WithIgnoreWhitespace()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = false;
            model.FindOptions.IgnoreWhitespaceCharacters = true;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = false;
            model.FindOptions.UseRegularExpressions = true;

            Hit match;
            string testText = "this    was some text";

            match = model.GetNextMatch_FOR_TESTING("th.*    was", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(11, match.Length);
        }




        [TestMethod]
        public void Match_Regex_WithIgnorePunctuation()//problem is ignore punct will destroy the regex in the query, could just not strip it in query??
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = true;
            model.FindOptions.IgnoreWhitespaceCharacters = false;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = false;
            model.FindOptions.UseRegularExpressions = true;

            Hit match;
            string testText = "this 'was some text";

            match = model.GetNextMatch_FOR_TESTING("th.* was", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(9, match.Length);
        }

        [TestMethod]
        public void Match_Wildcard_WithIgnorePunctuation()//problem is ignore punct will destroy the wildacard in the query, could just not strip it in query??
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = true;
            model.FindOptions.IgnoreWhitespaceCharacters = false;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = true;
            model.FindOptions.UseRegularExpressions = false;

            Hit match;
            string testText = "this 'was some text";

            //Because we cannot remove punct from the query when the match is wildcard or regex, we have to search without it.
            match = model.GetNextMatch_FOR_TESTING("th*s was", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(9, match.Length);

            //Because we cannot remove punct from the query when the match is wildcard or regex, we have to search without it.
            match = model.GetNextMatch_FOR_TESTING("this was", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(9, match.Length);
        }


      



        [TestMethod]
        public void Match_Wildcard_WithIgnoreWhitespace()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = false;
            model.FindOptions.IgnoreWhitespaceCharacters = true;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = true;
            model.FindOptions.UseRegularExpressions = false;

            Hit match;
            string testText = "this     was some text";

            match = model.GetNextMatch_FOR_TESTING("th*s was", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(12, match.Length);

            testText = "this   was some text";

            match = model.GetNextMatch_FOR_TESTING("this w*s", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(10, match.Length);
        }


        [TestMethod]
        public void OptionFlipping()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.Query = new Query("some more");
            model.FindOptions.IgnoreWhitespaceCharacters = false;
            List<Run> runs = new List<Run>();
            runs.Add(new Run("some more"));
            Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer container = new Keyoti.RapidFindReplace.WPF.FindHandlers.RunCollectionContainer(runs);

            model.FindText(container);
            model.SelectNextMatch();
            Assert.AreEqual(model.CurrentMatch.AbsoluteEnd, 9);
            Assert.AreEqual(model.CurrentMatch.AbsoluteStart, 0);



            model.FindOptions.IgnoreWhitespaceCharacters = true;
            model.FindOptions.IgnoreWhitespaceCharacters = false;

            model.Query.QueryText = "some more ";
            model.FindText(container);
            model.SelectNextMatch();


            model.Query.QueryText = "some more";
            model.FindText(container);
            model.SelectNextMatch();
            Assert.AreEqual(model.CurrentMatch.AbsoluteEnd, 9);
            Assert.AreEqual(model.CurrentMatch.AbsoluteStart, 0);

        }
    }
}
