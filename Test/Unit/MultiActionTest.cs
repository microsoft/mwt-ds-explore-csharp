﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary.MultiAction;
using System.Collections.Generic;
using System.Linq;

namespace ExploreTests.MultiAction
{
    [TestClass]
    public class MWTExploreTests
    {
        /* 
        ** C# Tests do not need to be as extensive as those for C++. These tests should ensure
        ** the interactions between managed and native code are as expected.
        */
        [TestMethod]
        public void MultiActionEpsilonGreedy()
        {
            uint numActions = 10;
            float epsilon = 0f; // no randomization
            var policy = new TestPolicy<TestContext>(numActions);
            var testContext = new TestContext();
            var explorer = new EpsilonGreedyExplorer<TestContext>(policy, epsilon, numActions);

            EpsilonGreedyWithContext(numActions, testContext, policy, explorer);
        }

        [TestMethod]
        public void MultiActionEpsilonGreedyFixedActionUsingVariableActionInterface()
        {
            uint numActions = 10;
            float epsilon = 0f; // no randomization
            var policy = new TestPolicy<TestVarContext>(numActions);
            var testContext = new TestVarContext(numActions);
            var explorer = new EpsilonGreedyExplorer<TestVarContext>(policy, epsilon);

            EpsilonGreedyWithContext(numActions, testContext, policy, explorer);
        }

        private static void EpsilonGreedyWithContext<TContext>(uint numActions, TContext testContext, TestPolicy<TContext> policy, IExplorer<TContext> explorer)
            where TContext : TestContext
        {
            string uniqueKey = "ManagedTestId";
            TestRecorder<TContext> recorder = new TestRecorder<TContext>();
            MwtExplorer<TContext> mwtt = new MwtExplorer<TContext>("mwt", recorder);
            testContext.Id = 100;

            uint[] expectedActions = policy.ChooseAction(testContext, numActions).Actions;

            uint[] chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext, numActions);
            Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));

            chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext, numActions);
            Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(2, interactions.Count);

            Assert.AreEqual(testContext.Id, interactions[0].Context.Id);

            // Verify that policy action is chosen all the time
            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext, numActions);
                Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));
            }
        }

        [TestMethod]
        public void MultiActionTauFirst()
        {
            uint numActions = 10;
            uint tau = 0;
            TestContext testContext = new TestContext() { Id = 100 };
            var policy = new TestPolicy<TestContext>(numActions);
            var explorer = new TauFirstExplorer<TestContext>(policy, tau, numActions);
            TauFirstWithContext(numActions, testContext, policy, explorer);
        }

        [TestMethod]
        public void MultiActionTauFirstFixedActionUsingVariableActionInterface()
        {
            uint numActions = 10;
            uint tau = 0;
            var testContext = new TestVarContext(numActions) { Id = 100 };
            var policy = new TestPolicy<TestVarContext>(numActions);
            var explorer = new TauFirstExplorer<TestVarContext>(policy, tau);
            TauFirstWithContext(numActions, testContext, policy, explorer);
        }

        private static void TauFirstWithContext<TContext>(uint numActions, TContext testContext, TestPolicy<TContext> policy, IExplorer<TContext> explorer)
            where TContext : TestContext
        {
            string uniqueKey = "ManagedTestId";

            var recorder = new TestRecorder<TContext>();
            var mwtt = new MwtExplorer<TContext>("mwt", recorder);

            uint[] expectedActions = policy.ChooseAction(testContext, numActions).Actions;

            uint[] chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext, numActions);
            Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(0, interactions.Count);

            // Verify that policy action is chosen all the time
            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext, numActions);
                Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));
            }
        }

        [TestMethod]
        public void MultiActionBootstrap()
        {
            uint numActions = 10;
            uint numbags = 2;
            TestContext testContext1 = new TestContext() { Id = 99 };
            TestContext testContext2 = new TestContext() { Id = 100 };

            var policies = new TestPolicy<TestContext>[numbags];
            for (int i = 0; i < numbags; i++)
            {
                policies[i] = new TestPolicy<TestContext>(numActions, i * 2);
            }
            var explorer = new BootstrapExplorer<TestContext>(policies, numActions);

            BootstrapWithContext(numActions, testContext1, testContext2, policies, explorer);
        }

        [TestMethod]
        public void MultiActionBootstrapFixedActionUsingVariableActionInterface()
        {
            uint numActions = 10;
            uint numbags = 2;
            var testContext1 = new TestVarContext(numActions) { Id = 99 };
            var testContext2 = new TestVarContext(numActions) { Id = 100 };

            var policies = new TestPolicy<TestVarContext>[numbags];
            for (int i = 0; i < numbags; i++)
            {
                policies[i] = new TestPolicy<TestVarContext>(numActions, i * 2);
            }
            var explorer = new BootstrapExplorer<TestVarContext>(policies);

            BootstrapWithContext(numActions, testContext1, testContext2, policies, explorer);
        }

        private static void BootstrapWithContext<TContext>(uint numActions, TContext testContext1, TContext testContext2, TestPolicy<TContext>[] policies, IExplorer<TContext> explorer)
            where TContext : TestContext
        {
            string uniqueKey = "ManagedTestId";

            var recorder = new TestRecorder<TContext>();
            var mwtt = new MwtExplorer<TContext>("mwt", recorder);

            uint[] expectedActions = policies[0].ChooseAction(testContext1, numActions).Actions;

            uint[] chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext1, numActions);
            Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));

            chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext2, numActions);
            Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(2, interactions.Count);

            Assert.AreEqual(testContext1.Id, interactions[0].Context.Id);
            Assert.AreEqual(testContext2.Id, interactions[1].Context.Id);

            // Verify that policy action is chosen all the time
            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext1, numActions);
                Assert.IsTrue(expectedActions.SequenceEqual(chosenActions));
            }
        }

        [TestMethod]
        public void MultiActionSoftmax()
        {
            uint numActions = 10;
            float lambda = 0.5f;
            uint numActionsCover = 100;
            float C = 5;
            var scorer = new TestScorer<TestContext>(numActions);
            var explorer = new SoftmaxExplorer<TestContext>(scorer, lambda, numActions);

            uint numDecisions = (uint)(numActions * Math.Log(numActions * 1.0) + Math.Log(numActionsCover * 1.0 / numActions) * C * numActions);
            var contexts = new TestContext[numDecisions];
            for (int i = 0; i < numDecisions; i++)
            {
                contexts[i] = new TestContext { Id = i };
            }

            SoftmaxWithContext(numActions, explorer, contexts);
        }

        [TestMethod]
        public void MultiActionSoftmaxFixedActionUsingVariableActionInterface()
        {
            uint numActions = 10;
            float lambda = 0.5f;
            uint numActionsCover = 100;
            float C = 5;
            var scorer = new TestScorer<TestVarContext>(numActions);
            var explorer = new SoftmaxExplorer<TestVarContext>(scorer, lambda);

            uint numDecisions = (uint)(numActions * Math.Log(numActions * 1.0) + Math.Log(numActionsCover * 1.0 / numActions) * C * numActions);
            var contexts = new TestVarContext[numDecisions];
            for (int i = 0; i < numDecisions; i++)
            {
                contexts[i] = new TestVarContext(numActions) { Id = i };
            }

            SoftmaxWithContext(numActions, explorer, contexts);
        }

        private static void SoftmaxWithContext<TContext>(uint numActions, IExplorer<TContext> explorer, TContext[] contexts)
            where TContext : TestContext
        {
            var recorder = new TestRecorder<TContext>();
            var mwtt = new MwtExplorer<TContext>("mwt", recorder);

            uint[] actions = new uint[numActions];

            Random rand = new Random();
            for (uint i = 0; i < contexts.Length; i++)
            {
                uint[] chosenActions = mwtt.ChooseAction(explorer, rand.NextDouble().ToString(), contexts[i], numActions);

                Assert.AreEqual(numActions, (uint)chosenActions.Length);

                // Make sure the list does not contain duplicate items
                Assert.AreEqual(chosenActions.Length, chosenActions.Distinct().Count());

                actions[chosenActions[0] - 1]++; // action id is one-based
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
        public void MultiActionSoftmaxScores()
        {
            uint numActions = 10;
            float lambda = 0.5f;
            var recorder = new TestRecorder<TestContext>();
            var scorer = new TestScorer<TestContext>(numActions, uniform: false);

            var mwtt = new MwtExplorer<TestContext>("mwt", recorder);
            var explorer = new SoftmaxExplorer<TestContext>(scorer, lambda, numActions);

            Random rand = new Random();
            uint[] chosenActions = mwtt.ChooseAction(explorer, rand.NextDouble().ToString(), new TestContext() { Id = 100 }, numActions);

            Assert.AreEqual(numActions, (uint)chosenActions.Length);
            Assert.AreEqual(chosenActions.Length, chosenActions.Distinct().Count());

            chosenActions = mwtt.ChooseAction(explorer, rand.NextDouble().ToString(), new TestContext() { Id = 101 }, numActions);

            Assert.AreEqual(numActions, (uint)chosenActions.Length);
            Assert.AreEqual(chosenActions.Length, chosenActions.Distinct().Count());

            chosenActions = mwtt.ChooseAction(explorer, rand.NextDouble().ToString(), new TestContext() { Id = 102 }, numActions);

            Assert.AreEqual(numActions, (uint)chosenActions.Length);
            Assert.AreEqual(chosenActions.Length, chosenActions.Distinct().Count());

            var interactions = recorder.GetAllInteractions();

            Assert.AreEqual(3, interactions.Count);

            for (int i = 0; i < interactions.Count; i++)
            {
                // Scores are not equal therefore probabilities should not be uniform
                Assert.AreNotEqual(interactions[i].Probability, 1.0f / numActions);
                Assert.AreEqual(100 + i, interactions[i].Context.Id);
            }

            // Verify that the highest-score action is chosen all the time
            TestContext context = new TestContext { Id = 100 };
            List<float> scores = scorer.ScoreActions(context);
            float maxScore = 0;
            uint highestScoreAction = 0;
            for (int i = 0; i < scores.Count; i++)
            {
                if (maxScore < scores[i])
                {
                    maxScore = scores[i];
                    highestScoreAction = (uint)i + 1;
                }
            }

            explorer.EnableExplore(false);
            for (int i = 0; i < 1000; i++)
            {
                chosenActions = mwtt.ChooseAction(explorer, rand.NextDouble().ToString(), new TestContext() { Id = (int)i }, numActions);
                Assert.AreEqual(numActions, (uint)chosenActions.Length);
                Assert.AreEqual(chosenActions.Length, chosenActions.Distinct().Count());

                Assert.AreEqual(highestScoreAction, chosenActions[0]);
            }
        }

        [TestMethod]
        public void MultiActionGeneric()
        {
            uint numActions = 10;
            TestScorer<TestContext> scorer = new TestScorer<TestContext>(numActions);
            TestContext testContext = new TestContext() { Id = 100 };
            var explorer = new GenericExplorer<TestContext>(scorer, numActions);
            GenericWithContext(numActions, testContext, explorer);
        }

        [TestMethod]
        public void MultiActionGenericFixedActionUsingVariableActionInterface()
        {
            uint numActions = 10;
            var scorer = new TestScorer<TestVarContext>(numActions);
            var testContext = new TestVarContext(numActions) { Id = 100 };
            var explorer = new GenericExplorer<TestVarContext>(scorer);
            GenericWithContext(numActions, testContext, explorer);
        }

        private static void GenericWithContext<TContext>(uint numActions, TContext testContext, IExplorer<TContext> explorer)
            where TContext : TestContext
        {
            string uniqueKey = "ManagedTestId";
            var recorder = new TestRecorder<TContext>();

            var mwtt = new MwtExplorer<TContext>("mwt", recorder);

            uint[] chosenActions = mwtt.ChooseAction(explorer, uniqueKey, testContext, numActions);
            Assert.AreEqual(numActions, (uint)chosenActions.Length);
            Assert.AreEqual(chosenActions.Length, chosenActions.Distinct().Count());

            var interactions = recorder.GetAllInteractions();
            Assert.AreEqual(1, interactions.Count);
            Assert.AreEqual(testContext.Id, interactions[0].Context.Id);
        }

        [TestMethod]
        public void MultiActionUsageBadVariableActionContext()
        {
            int numExceptionsCaught = 0;
            int numExceptionsExpected = 5;

            var tryCatchArgumentException = (Action<Action>)((action) =>
            {
                try
                {
                    action();
                }
                catch (InvalidOperationException)
                {
                    numExceptionsCaught++;
                }
                catch (ArgumentException)
                {
                    numExceptionsCaught++;
                }
            });

            tryCatchArgumentException(() =>
            {
                var mwt = new MwtExplorer<TestContext>("test", new TestRecorder<TestContext>());
                var policy = new TestPolicy<TestContext>(1);
                var explorer = new EpsilonGreedyExplorer<TestContext>(policy, 0.2f);
                mwt.ChooseAction(explorer, "key", new TestContext());
            });
            tryCatchArgumentException(() =>
            {
                var mwt = new MwtExplorer<TestContext>("test", new TestRecorder<TestContext>());
                var policy = new TestPolicy<TestContext>(1);
                var explorer = new TauFirstExplorer<TestContext>(policy, 10);
                mwt.ChooseAction(explorer, "key", new TestContext());
            });
            tryCatchArgumentException(() =>
            {
                var mwt = new MwtExplorer<TestContext>("test", new TestRecorder<TestContext>());
                var policies = new TestPolicy<TestContext>[2];
                for (int i = 0; i < 2; i++)
                {
                    policies[i] = new TestPolicy<TestContext>(1, i * 2);
                }
                var explorer = new BootstrapExplorer<TestContext>(policies);
                mwt.ChooseAction(explorer, "key", new TestContext());
            });
            tryCatchArgumentException(() =>
            {
                var mwt = new MwtExplorer<TestContext>("test", new TestRecorder<TestContext>());
                var scorer = new TestScorer<TestContext>(10);
                var explorer = new SoftmaxExplorer<TestContext>(scorer, 0.5f);
                mwt.ChooseAction(explorer, "key", new TestContext());
            });
            tryCatchArgumentException(() =>
            {
                var mwt = new MwtExplorer<TestContext>("test", new TestRecorder<TestContext>());
                var scorer = new TestScorer<TestContext>(10);
                var explorer = new GenericExplorer<TestContext>(scorer);
                mwt.ChooseAction(explorer, "key", new TestContext());
            });

            Assert.AreEqual(numExceptionsExpected, numExceptionsCaught);
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

    public interface IVariableActionContext
    {
        uint GetNumberOfActions();
    }

    struct TestInteraction<Ctx>
    {
        public Ctx Context;
        public UInt32[] Actions;
        public float Probability;
        public string UniqueKey;
    }

    class TestContext
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
    }

    class TestVarContext : TestContext, IVariableActionContext
    {
        public TestVarContext(uint numberOfActions)
        {
            NumberOfActions = numberOfActions;
        }

        public uint GetNumberOfActions()
        {
            return NumberOfActions;
        }

        public uint NumberOfActions { get; set; }
    }

    class TestRecorder<Ctx> : IRecorder<Ctx>
    {
        public void Record(Ctx context, UInt32[] actions, float probability, UniqueEventID uniqueKey, string modelId = null, bool? isExplore = null)
        {
            interactions.Add(new TestInteraction<Ctx>()
            {
                Context = context,
                Actions = actions,
                Probability = probability,
                UniqueKey = uniqueKey.Key
            });
        }

        public List<TestInteraction<Ctx>> GetAllInteractions()
        {
            return interactions;
        }

        private List<TestInteraction<Ctx>> interactions = new List<TestInteraction<Ctx>>();
    }

    class TestPolicy<TContext> : IPolicy<TContext>
    {
        public TestPolicy(uint numberOfActions) : this(numberOfActions, -1) { }

        public TestPolicy(uint numberOfActions, int index)
        {
            this.numberOfActions = numberOfActions;
            this.index = index;
        }
        
        public PolicyDecisionTuple ChooseAction(TContext context, uint numActionsVariable = uint.MaxValue)
        {
            uint[] actions = new uint[numberOfActions];
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i] = (uint)(i + 1);
            }
            return new PolicyDecisionTuple { Actions = actions };
        }

        private int index;
        private uint numberOfActions;
    }

    class TestScorer<Ctx> : IScorer<Ctx>
    {
        public TestScorer(uint numActions, bool uniform = true)
        {
            this.uniform = uniform;
            this.numActions = numActions;
        }
        public List<float> ScoreActions(Ctx context, uint numActionsVariable = uint.MaxValue)
        {
            if (uniform)
            {
                return Enumerable.Repeat<float>(1.0f / numActions, (int)numActions).ToList();
            }
            else
            {
                return Array.ConvertAll<int, float>(Enumerable.Range(1, (int)numActions).ToArray(), Convert.ToSingle).ToList();
            }
        }
        private uint numActions;
        private bool uniform;
    }
}
