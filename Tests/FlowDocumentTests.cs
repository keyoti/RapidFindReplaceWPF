using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Keyoti.RapidFindReplace.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Keyoti.RapidFindReplace.WPF.FindHandlers;
using Keyoti.RapidFindReplace.WPF.FindHandlers.RunReaders;
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;

namespace Tests
{
    [TestClass]
    public class FlowDocumentTests
    {
        
        String testFilesPath = @"..\..\TestFiles";

        [TestMethod]
        public void FlowProblem1()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            bool hit=false;
            FlowDocument content = LoadFlowDocument("FlowDocument1.xaml");
            System.Collections.Generic.List<Run> runs = new System.Collections.Generic.List<Run>(5);
            FindRuns(content.Blocks, runs);
            finder.FindTextIn(runs, delegate(Run run, int index, int length)
                {
                    hit = true;
                },   new Query("urna jimmy"));
            
            Assert.IsTrue(hit);
        }

        [TestMethod]
        public void ReplaceReplaceAllProblem1()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            FlowDocument content = LoadFlowDocument("FlowDocument1.xaml");
            System.Collections.Generic.List<Run> runs = new System.Collections.Generic.List<Run>(5);
            //FindRuns(content.Blocks, runs);
            runs.Add(new Run("aaaa"));

            RunCollectionContainer coll = new RunCollectionContainer(runs);

           
            finder.Query = new Query("aaaa");
            finder.FindText(coll);

           
            
            finder.ReplaceMatchCommand.Execute("ddd");
            finder.ReplaceAllMatchesCommand.Execute("ddd");
            
        }


        [TestMethod]
        public void FindDoubleSpace()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
           
            System.Collections.Generic.List<Run> runs = new System.Collections.Generic.List<Run>(5);
            //FindRuns(content.Blocks, runs);
            runs.Add(new Run("a  a"));
            runs.Add(new Run("T"));

            RunCollectionContainer coll = new RunCollectionContainer(runs);

 
            finder.Query = new Query("  ");
            finder.FindText(coll);

            finder.SelectNextMatch();
            
         
            Assert.AreEqual(1, finder.CurrentMatch.AbsoluteStart);
            Assert.AreEqual(3, finder.CurrentMatch.AbsoluteEnd);
            
        }

        [TestMethod]
        public void ReplaceAsFindWithPrependSpace()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            

            System.Collections.Generic.List<Run> runs = new System.Collections.Generic.List<Run>(5);
            //FindRuns(content.Blocks, runs);
            runs.Add(new Run("ax  ax"));
            runs.Add(new Run("T"));

            RunCollectionContainer coll = new RunCollectionContainer(runs);


            finder.Query = new Query("ax");
            finder.FindText(coll);

          

            finder.ReplaceAllMatchesCommand.Execute(" ax");
           
        }

        [TestMethod]
        public void ReplaceAllAsFindWithPrependSpace()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();


            System.Collections.Generic.List<Run> runs = new System.Collections.Generic.List<Run>(5);
            //FindRuns(content.Blocks, runs);
            runs.Add(new Run("ax ax"));
            runs.Add(new Run("T"));

            RunCollectionContainer coll = new RunCollectionContainer(runs);


            finder.Query = new Query("ax");
            finder.FindText(coll);

            //finder.SelectNextMatch();

            finder.ReplaceAllMatchesCommand.Execute(" ax");
            runs[0].Text = " ax  ax";

            
        }

        [TestMethod]
        public void FlowProblem_Jimmy_SecondColumn_no2()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            FlowDocument content = LoadFlowDocument("FlowDocument4.xaml");
            int hitCount = 0;
            finder.FindTextIn(new FlowDocumentRunReader(content), delegate(Run run, int index, int length)
            {
                hitCount++;
            }, new Query("jimmy"));

            Assert.AreEqual(2, hitCount);
        }

        [TestMethod]
        public void FlowProblem1_AsFlowDocument()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            bool hit = false;
            FlowDocument content = LoadFlowDocument("FlowDocument1.xaml");
            finder.FindTextIn(new FlowDocumentRunReader(content), delegate(Run run, int index, int length)
            {
                hit = true;
            }, new Query("urna jimmy"));

            Assert.IsTrue(hit);
        }

        [TestMethod]
        public void FlowProblem1_AsFlowDocument_WithRegistry()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            bool hit = false;
            FlowDocument content = LoadFlowDocument("FlowDocument1.xaml");
            finder.FindTextIn(FindHandlerRegistry.CreateIRunReaderHandlerFor(content), delegate(Run run, int index, int length)
            {
                hit = true;
            }, new Query("urna jimmy"));

            Assert.IsTrue(hit);
        }

        [TestMethod]
        public void Flow_IgnoreWhitespace()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
            finder.FindOptions.IgnoreWhitespaceCharacters = true;
            bool hit = false;
            FlowDocument content = LoadFlowDocument("FlowDocument2.xaml");
            finder.FindTextIn(FindHandlerRegistry.CreateIRunReaderHandlerFor(content), delegate(Run run, int index, int length)
            {
                hit = true;
            }, new Query("mus1. Aliquam1"));

            Assert.IsTrue(hit);
        }

        [TestMethod]
        public void Flow_3()
        {
            RapidFindReplaceControlViewModel finder = new RapidFindReplaceControlViewModel();
           // finder.FindOptions.IgnoreWhitespaceCharacters = true;
            bool hit = false;
            FlowDocument content = LoadFlowDocument("FlowDocument3.xaml");
            finder.FindTextIn(FindHandlerRegistry.CreateIRunReaderHandlerFor(content), delegate(Run run, int index, int length)
            {
                hit = true;
            }, new Query("a b c"));

            Assert.IsTrue(hit);
        }


        

        //is this robust enough for general use?  'block is Section' is enough? block is Paragraph is enough?
        private static void FindRuns(BlockCollection content, System.Collections.Generic.List<Run> runs)
        {
            foreach (Block block in content)
            {
                //block.child
                if (block is Paragraph)
                {
                    foreach (Inline r in (block as Paragraph).Inlines)
                    {
                        if(r is Run)
                            runs.Add(r as Run);
                        
                
                    }
                }
                else if (block is Section)
                {
                    FindRuns((block as Section).Blocks, runs);
                }
            }
        }

        private FlowDocument LoadFlowDocument(string xamlName)
        {
            FlowDocument content = null;
            FileStream xamlFile = File.Open(Path.Combine(testFilesPath, this.GetType().Name + "\\"+xamlName), FileMode.Open);


            content = XamlReader.Load(xamlFile) as FlowDocument;
            if (content == null)
                throw (new XamlParseException("The specified file could not be loaded as a FlowDocument."));
            return content;
        }


    }
}
