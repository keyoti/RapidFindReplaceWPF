using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Keyoti.RapidFindReplace.WPF;
using Keyoti.RapidFindReplace.WPF.TextMatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class IgnoreHandlerTests
    {
        [TestMethod]
        public void WhitespaceIgnorer()
        {
            IgnoreCharacterClassHandler handler = new IgnoreCharacterClassHandler(IgnoreCharacterClassHandler.CharacterClass.Whitespace);

            string input = "012345 789A  BCDEF\r\nnewline";
            int dummy;
            string output = handler.TranslateToIgnored(input, 0, out dummy);
            Assert.AreEqual("012345789ABCDEFnewline", output);

            Hit h = handler.TranslateHit(new Hit(0,2));
            Assert.AreEqual(0, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(1, 2));
            Assert.AreEqual(1, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(2, 2));
            Assert.AreEqual(2, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(3, 2));
            Assert.AreEqual(3, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(4, 2));
            Assert.AreEqual(4, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(4, 3));
            Assert.AreEqual(4, h.Start);
            Assert.AreEqual(4, h.Length);

            h = handler.TranslateHit(new Hit(5, 2));
            Assert.AreEqual(5, h.Start);
            Assert.AreEqual(3, h.Length);

            h = handler.TranslateHit(new Hit(6, 2));
            Assert.AreEqual(7, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(6, 2));
            Assert.AreEqual(7, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(7, 2));
            Assert.AreEqual(8, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(8, 2));
            Assert.AreEqual(9, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(9, 2));
            Assert.AreEqual(10, h.Start);
            Assert.AreEqual(4, h.Length);

            h = handler.TranslateHit(new Hit(10, 2));
            Assert.AreEqual(13, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(15, 2));
            Assert.AreEqual(20, h.Start);
            Assert.AreEqual(2, h.Length);

        }

        [TestMethod]
        public void PunctuationIgnorer()
        {
            IgnoreCharacterClassHandler handler = new IgnoreCharacterClassHandler(IgnoreCharacterClassHandler.CharacterClass.Punctuation);

            string input = "012345\"789A'?BCDEF,.newline";
            int dummy;
            string output = handler.TranslateToIgnored(input, 0, out dummy);
            Assert.AreEqual("012345789ABCDEFnewline", output);

            Hit h = handler.TranslateHit(new Hit(0, 2));
            Assert.AreEqual(0, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(1, 2));
            Assert.AreEqual(1, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(2, 2));
            Assert.AreEqual(2, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(3, 2));
            Assert.AreEqual(3, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(4, 2));
            Assert.AreEqual(4, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(4, 3));
            Assert.AreEqual(4, h.Start);
            Assert.AreEqual(4, h.Length);

            h = handler.TranslateHit(new Hit(5, 2));
            Assert.AreEqual(5, h.Start);
            Assert.AreEqual(3, h.Length);

            h = handler.TranslateHit(new Hit(6, 2));
            Assert.AreEqual(7, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(6, 2));
            Assert.AreEqual(7, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(7, 2));
            Assert.AreEqual(8, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(8, 2));
            Assert.AreEqual(9, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(9, 2));
            Assert.AreEqual(10, h.Start);
            Assert.AreEqual(4, h.Length);

            h = handler.TranslateHit(new Hit(10, 2));
            Assert.AreEqual(13, h.Start);
            Assert.AreEqual(2, h.Length);

            h = handler.TranslateHit(new Hit(15, 2));
            Assert.AreEqual(20, h.Start);
            Assert.AreEqual(2, h.Length);

        }



        [TestMethod]
        public void PunctuationAndWhitespaceIgnorer()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = true;
            model.FindOptions.IgnoreWhitespaceCharacters = true;
            model.FindOptions.MatchCase = false;
            model.FindOptions.MatchPrefix = false;
            model.FindOptions.MatchSuffix = false;
            model.FindOptions.UseWildcards = false;
            model.FindOptions.UseRegularExpressions = false;

            Hit match;
            string testText = "this was some text";

            match = model.GetNextMatch_FOR_TESTING("thiswas", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

            match = model.GetNextMatch_FOR_TESTING("this was", 0, testText);
            Assert.AreEqual(0, match.Start);
            Assert.AreEqual(8, match.Length);

        }


        [TestMethod]
        public void PunctuationIgnorer_FromModel()
        {
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.FindWholeWordsOnly = false;
            model.FindOptions.IgnorePunctuationCharacters = true;
            


            Hit match;
            string testText = "this was-nt some text";

            match = model.GetNextMatch_FOR_TESTING("was-nt", 0, testText);
            Assert.AreEqual(5, match.Start);
            Assert.AreEqual(6, match.Length);



        }


        [TestMethod]
        public void MultipleIgnoresWithRuns()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("Some rich text box content"));
            runs.Add(new Run("some rich text bo"));
            runs.Add(new Run("55555 6666"));


            //harder one
            int callInc = 0;
            RunQueue rq2 = new RunQueue(5);
            QueueTests.FillRunQueue(rq2, runs);
            RunQueue rq2_clone = rq2.Clone();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.IgnoreWhitespaceCharacters = true;
            model.ScanQueuedRuns(rq2, "some rich text box",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc < 1);
                    if (callInc == 0)
                    {
                        Assert.AreEqual(rq2_clone[0].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(18, length);
                    }
                    callInc++;

                }
            );
            

        }

        [TestMethod]
        public void IgnoreWhitespaceSplitRuns()
        {
            List<Run> runs = new List<Run>();
            runs.Add(new Run("  a b"));
            runs.Add(new Run(" c"));
            runs.Add(new Run(" stella was here a b "));
            runs.Add(new Run("but not here"));
            runs.Add(new Run("ok she's back a b"));


            //harder one
            int callInc = 0;
            RunQueue rq2 = new RunQueue(5);
            QueueTests.FillRunQueue(rq2, runs);
            RunQueue rq2_clone = rq2.Clone();
            RapidFindReplaceControlViewModel model = new RapidFindReplaceControlViewModel();
            model.FindOptions.IgnoreWhitespaceCharacters = true;
            model.ScanQueuedRuns(rq2, "abc",
                delegate(Run run, int index, int length)
                {
                    Assert.IsTrue(callInc < 2);
                    if (callInc == 0)
                    {
                        Assert.AreEqual(rq2_clone[0].Run, run);
                        Assert.AreEqual(2, index);
                        Assert.AreEqual(3, length);
                    }
                    if (callInc == 1)
                    {
                        Assert.AreEqual(rq2_clone[1].Run, run);
                        Assert.AreEqual(0, index);
                        Assert.AreEqual(2, length);
                    }
                    callInc++;

                }
            );

            callInc = 0;
            model.ScanQueuedRuns(rq2, "a b c",
                            delegate(Run run, int index, int length)
                            {
                                Assert.IsTrue(callInc < 2);
                                if (callInc == 0)
                                {
                                    Assert.AreEqual(rq2_clone[0].Run, run);
                                    Assert.AreEqual(2, index);
                                    Assert.AreEqual(3, length);
                                }
                                if (callInc == 1)
                                {
                                    Assert.AreEqual(rq2_clone[1].Run, run);
                                    Assert.AreEqual(0, index);
                                    Assert.AreEqual(2, length);
                                }
                                callInc++;

                            }
                        );


        }

    }
}
