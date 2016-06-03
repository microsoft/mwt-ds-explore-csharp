using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary.SingleAction;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary;

namespace ExploreTests.SingleAction
{
    [TestClass]
    public class MWTExploreTests
    {
        /* 
        ** C# Tests do not need to be as extensive as those for C++. These tests should ensure
        ** the interactions between managed and native code are as expected.
        */
        [TestMethod]
        public void EpsilonGreedy()
        {
            int numActions = 10;
            float epsilon = 0f;
            var policy = new TestPolicy<RegularTestContext>();
            var testContext = new RegularTestContext();
            var explorer = new EpsilonGreedyExplorer(epsilon);

            EpsilonGreedyWithContext(numActions, testContext, policy, explorer);
        }

        [TestMethod]
        public void EpsilonGreedyFixedActionUsingVariableActionInterface()
        {
            int numActions = 10;
            float epsilon = 0f;
            var policy = new TestPolicy<VariableActionTestContext>();
            var testContext = new VariableActionTestContext(numActions);
            var explorer = new EpsilonGreedyExplorer(epsilon);

            EpsilonGreedyWithContext(numActions, testContext, policy, explorer);
        }

        private static void EpsilonGreedyWithContext<TContext>(int numActions, TContext testContext, TestPolicy<TContext> policy, IExplorer<int, int> explorer)
            where TContext : RegularTestContext
        {
            string uniqueKey = "ManagedTestId";
            var uniqueId = uniqueKey;
            TestRecorder<TContext> recorder = new TestRecorder<TContext>();
            //MwtExplorer<TContext> mwtt = new MwtExplorer<TContext>("mwt", recorder);
            var mwtt = MwtExplorer.Create("mwt", numActions, recorder, explorer, policy);
            testContext.Id = 100;

            int expectedAction = policy.MapContext(testContext).Value;

            int chosenAction = mwtt.ChooseAction(uniqueId, testContext);
            Assert.AreEqual(expectedAction, chosenAction);

            chosenAction = mwtt.ChooseAction(uniqueId, testContext);
            Assert.AreEqual(expectedAction, chosenAction);

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(2, interactions.Count);

            Assert.AreEqual(testContext.Id, interactions[0].Context.Id);

            // Verify that policy action is chosen all the time
            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                chosenAction = mwtt.ChooseAction(uniqueId, testContext);
                Assert.AreEqual(expectedAction, chosenAction);
            }
        }

        [TestMethod]
        public void TauFirst()
        {
            int numActions = 10;
            int tau = 0;
            RegularTestContext testContext = new RegularTestContext() { Id = 100 };
            var policy = new TestPolicy<RegularTestContext>();
            var explorer = new TauFirstExplorer(tau);
            TauFirstWithContext(numActions, testContext, policy, explorer);
        }

        [TestMethod]
        public void TauFirstFixedActionUsingVariableActionInterface()
        {
            int numActions = 10;
            int tau = 0;
            var testContext = new VariableActionTestContext(numActions) { Id = 100 };
            var policy = new TestPolicy<VariableActionTestContext>();
            var explorer = new TauFirstExplorer(tau);
            TauFirstWithContext(numActions, testContext, policy, explorer);
        }

        private static void TauFirstWithContext<TContext>(int numActions, TContext testContext, TestPolicy<TContext> policy, IExplorer<int, int> explorer)
            where TContext : RegularTestContext
        {
            string uniqueKey = "ManagedTestId";
            var uniqueId = uniqueKey;

            var recorder = new TestRecorder<TContext>();

            var mwtt = MwtExplorer.Create("mwt", numActions, recorder, explorer, policy);
            int expectedAction = policy.MapContext(testContext).Value;

            int chosenAction = mwtt.ChooseAction(uniqueId, testContext, numActions);
            Assert.AreEqual(expectedAction, chosenAction);

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(1, interactions.Count);

            // Verify that policy action is chosen all the time
            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                chosenAction = mwtt.ChooseAction(uniqueId, testContext);
                Assert.AreEqual(expectedAction, chosenAction);
            }
        }

        // TODO: Bootstrap is not fully supported by MWT at the moment
        //[TestMethod]
        //public void Bootstrap()
        //{
        //    uint numActions = 10;
        //    uint numbags = 2;
        //    RegularTestContext testContext1 = new RegularTestContext() { Id = 99 };
        //    RegularTestContext testContext2 = new RegularTestContext() { Id = 100 };

        //    var policies = new TestPolicy<RegularTestContext>[numbags];
        //    for (int i = 0; i < numbags; i++)
        //    {
        //        policies[i] = new TestPolicy<RegularTestContext>(i * 2);
        //    }
        //    var explorer = new BootstrapExplorer<RegularTestContext>(policies, numActions);

        //    BootstrapWithContext(numActions, testContext1, testContext2, policies, explorer);
        //}

        //[TestMethod]
        //public void BootstrapFixedActionUsingVariableActionInterface()
        //{
        //    uint numActions = 10;
        //    uint numbags = 2;
        //    var testContext1 = new VariableActionTestContext(numActions) { Id = 99 };
        //    var testContext2 = new VariableActionTestContext(numActions) { Id = 100 };

        //    var policies = new TestPolicy<VariableActionTestContext>[numbags];
        //    for (int i = 0; i < numbags; i++)
        //    {
        //        policies[i] = new TestPolicy<VariableActionTestContext>(i * 2);
        //    }
        //    var explorer = new BootstrapExplorer<VariableActionTestContext>(policies);

        //    BootstrapWithContext(numActions, testContext1, testContext2, policies, explorer);
        //}

        //private static void BootstrapWithContext<TContext>(uint numActions, TContext testContext1, TContext testContext2, TestPolicy<TContext>[] policies, IExplorer<TContext> explorer)
        //    where TContext : RegularTestContext
        //{
        //    string uniqueKey = "ManagedTestId";

        //    var recorder = new TestRecorder<TContext>();
        //    var mwtt = new MwtExplorer<TContext>("mwt", recorder);

        //    uint expectedAction = policies[0].ChooseAction(testContext1, numActions).Action;

        //    uint chosenAction = mwtt.ChooseAction(explorer, uniqueKey, testContext1, numActions);
        //    Assert.AreEqual(expectedAction, chosenAction);

        //    chosenAction = mwtt.ChooseAction(explorer, uniqueKey, testContext2, numActions);
        //    Assert.AreEqual(expectedAction, chosenAction);

        //    var interactions = recorder.GetAllInteractions();
        //    Assert.AreEqual(2, interactions.Count);

        //    Assert.AreEqual(testContext1.Id, interactions[0].Context.Id);
        //    Assert.AreEqual(testContext2.Id, interactions[1].Context.Id);

        //    // Verify that policy action is chosen all the time
        //    explorer.EnableExplore(false);
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        chosenAction = mwtt.ChooseAction(explorer, uniqueKey, testContext1, numActions);
        //        Assert.AreEqual(expectedAction, chosenAction);
        //    }
        //}

        [TestMethod]
        public void Softmax()
        {
            int numActions = 10;
            float lambda = 0.5f;
            int numActionsCover = 100;
            float C = 5;
            var scorer = new TestScorer<RegularTestContext>(1, numActions);
            var explorer = new SoftmaxExplorer(lambda);
            
            uint numDecisions = (uint)(numActions * Math.Log(numActions * 1.0) + Math.Log(numActionsCover * 1.0 / numActions) * C * numActions);
            var contexts = new RegularTestContext[numDecisions];
            for (int i = 0; i < numDecisions; i++)
            {
                contexts[i] = new RegularTestContext { Id = i };
            }

            SoftmaxWithContext(numActions, explorer, scorer, contexts);
        }

        [TestMethod]
        public void SoftmaxFixedActionUsingVariableActionInterface()
        {
            int numActions = 10;
            float lambda = 0.5f;
            int numActionsCover = 100;
            float C = 5;
            var scorer = new TestScorer<VariableActionTestContext>(1, numActions);
            var explorer = new SoftmaxExplorer(lambda);

            int numDecisions = (int)(numActions * Math.Log(numActions * 1.0) + Math.Log(numActionsCover * 1.0 / numActions) * C * numActions);
            var contexts = new VariableActionTestContext[numDecisions];
            for (int i = 0; i < numDecisions; i++)
            {
                contexts[i] = new VariableActionTestContext(numActions) { Id = i };
            }
            
            SoftmaxWithContext(numActions, explorer, scorer, contexts);
        }

        private static void SoftmaxWithContext<TContext>(int numActions, IExplorer<int, float[]> explorer, IContextMapper<TContext, float[]> scorer, TContext[] contexts)
            where TContext : RegularTestContext
        {
            var recorder = new TestRecorder<TContext>();
            //var mwtt = new MwtExplorer<TContext>("mwt", recorder);
            var mwtt = MwtExplorer.Create("mwt", numActions, recorder, explorer, scorer);

            uint[] actions = new uint[numActions];

            Random rand = new Random();
            for (uint i = 0; i < contexts.Length; i++)
            {
                var uniqueId = rand.NextDouble().ToString();
                int chosenAction = mwtt.ChooseAction(uniqueId, contexts[i]);
                actions[chosenAction - 1]++; // action id is one-based
            }

            for (uint i = 0; i < numActions; i++)
            {
                Assert.IsTrue(actions[i] > 0);
            }

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(contexts.Length, interactions.Count);

            for (int i = 0; i < contexts.Length; i++)
            {
                Assert.AreEqual(i, interactions[i].Context.Id);
            }
        }

        [TestMethod]
        public void SoftmaxScores()
        {
            int numActions = 10;
            float lambda = 0.5f;
            var recorder = new TestRecorder<RegularTestContext>();
            var scorer = new TestScorer<RegularTestContext>(1, numActions, uniform: false);

            //var mwtt = new MwtExplorer<RegularTestContext>("mwt", recorder);
            var explorer = new SoftmaxExplorer(lambda);

            var mwtt = MwtExplorer.Create("mwt", numActions, recorder, explorer, scorer);

            Random rand = new Random();
            mwtt.ChooseAction(rand.NextDouble().ToString(), new RegularTestContext() { Id = 100 });
            mwtt.ChooseAction(rand.NextDouble().ToString(), new RegularTestContext() { Id = 101 });
            mwtt.ChooseAction(rand.NextDouble().ToString(), new RegularTestContext() { Id = 102 });

            var interactions = recorder.GetAllInteractions();
            
            Assert.AreEqual(3, interactions.Count);

            for (int i = 0; i < interactions.Count; i++)
            {
                // Scores are not equal therefore probabilities should not be uniform
                Assert.AreNotEqual(interactions[i].Probability, 1.0f / numActions);
                Assert.AreEqual(100 + i, interactions[i].Context.Id);
            }

            // Verify that policy action is chosen all the time
            RegularTestContext context = new RegularTestContext { Id = 100 };
            List<float> scores = scorer.MapContext(context).Value.ToList();
            float maxScore = 0;
            int highestScoreAction = 0;
            for (int i = 0; i < scores.Count; i++)
            {
                if (maxScore < scores[i])
                {
                    maxScore = scores[i];
                    highestScoreAction = i + 1;
                }
            }

            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                int chosenAction = mwtt.ChooseAction(rand.NextDouble().ToString(), new RegularTestContext() { Id = (int)i });
                Assert.AreEqual(highestScoreAction, chosenAction);
            }
        }

        [TestMethod]
        public void Generic()
        {
            int numActions = 10;
            var scorer = new TestScorer<RegularTestContext>(1, numActions);
            RegularTestContext testContext = new RegularTestContext() { Id = 100 };
            var explorer = new GenericExplorer();
            GenericWithContext(numActions, testContext, explorer, scorer);
        }

        [TestMethod]
        public void GenericFixedActionUsingVariableActionInterface()
        {
            int numActions = 10;
            var scorer = new TestScorer<VariableActionTestContext>(1, numActions);
            var testContext = new VariableActionTestContext(numActions) { Id = 100 };
            var explorer = new GenericExplorer();
            GenericWithContext(numActions, testContext, explorer, scorer);
        }

        private static void GenericWithContext<TContext>(int numActions, TContext testContext, IExplorer<int, float[]> explorer, IContextMapper<TContext, float[]> scorer)
            where TContext : RegularTestContext
        {
            string uniqueKey = "ManagedTestId";
            var recorder = new TestRecorder<TContext>();

            //var mwtt = new MwtExplorer<TContext>("mwt", recorder);
            var mwtt = MwtExplorer.Create("mwt", numActions, recorder, explorer, scorer);

            int chosenAction = mwtt.ChooseAction(uniqueKey, testContext);

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(1, interactions.Count);
            Assert.AreEqual(testContext.Id, interactions[0].Context.Id);
        }

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
    }

    struct TestInteraction<Ctx>
    { 
        public Ctx Context; 
        public UInt32 Action;
        public float Probability;
        public string UniqueKey;
    }

    class TestRecorder<Ctx> : IRecorder<Ctx, int>
    {
        public void Record(Ctx context, int value, object explorerState, object mapperState, string uniqueKey)
        {
            interactions.Add(new TestInteraction<Ctx>()
            { 
                Context = context,
                Action = (uint)value,
                Probability = ((GenericExplorerState)explorerState).Probability,
                UniqueKey = uniqueKey
            });
        }

        public List<TestInteraction<Ctx>> GetAllInteractions()
        {
            return interactions;
        }

        private List<TestInteraction<Ctx>> interactions = new List<TestInteraction<Ctx>>();
    }
}
