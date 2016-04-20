using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary.SingleAction;

namespace cs_test
{
    class ExploreOnlySample
    {
        /// <summary>
        /// Example of a custom context.
        /// </summary>
        class MyContext { }

        /// <summary>
        /// Example of a custom recorder which implements the IRecorder<MyContext>,
        /// declaring that this recorder only interacts with MyContext objects.
        /// </summary>
        class MyRecorder : IRecorder<MyContext, int>
        {
            public void Record(MyContext context, int value, object explorerState, object mapperState, UniqueEventID uniqueKey)
            {
                // Stores the tuple internally in a vector that could be used later for other purposes.
                interactions.Add(new Interaction<MyContext>()
                {
                    Context = context,
                    Action = (uint)value,
                    Probability = ((GenericExplorerState)explorerState).Probability,
                    UniqueKey = uniqueKey.Key
                });
            }

            public List<Interaction<MyContext>> GetAllInteractions()
            {
                return interactions;
            }

            private List<Interaction<MyContext>> interactions = new List<Interaction<MyContext>>();

            
        }

        /// <summary>
        /// Example of a custom policy which implements the IPolicy<MyContext>,
        /// declaring that this policy only interacts with MyContext objects.
        /// </summary>
        class MyPolicy : IPolicy<MyContext>
        {
            public MyPolicy() : this(-1) { }

            public MyPolicy(int index)
            {
                this.index = index;
            }

            public PolicyDecision<int> MapContext(MyContext context)
            {
                // Always returns the same action regardless of context
                return 5;
            }

            private int index;
        }

        /// <summary>
        /// Example of a custom policy which implements the IPolicy<SimpleContext>,
        /// declaring that this policy only interacts with SimpleContext objects.
        /// </summary>
        class StringPolicy : IPolicy<SimpleContext>
        {
            public PolicyDecision<int> MapContext(SimpleContext context)
            {
                // Always returns the same action regardless of context
                return 1;
            }
        }

        /// <summary>
        /// Example of a custom scorer which implements the IScorer<MyContext>,
        /// declaring that this scorer only interacts with MyContext objects.
        /// </summary>
        class MyScorer : IScorer<MyContext>
        {
            private uint numActions;

            public MyScorer(uint numActions)
            {
                this.numActions = numActions;
            }

            public PolicyDecision<float[]> MapContext(MyContext context)
            {
                return Enumerable.Repeat<float>(1.0f / numActions, (int)numActions).ToArray();
            }
        }

        /// <summary>
        /// Represents a tuple <context, action, probability, key>.
        /// </summary>
        /// <typeparam name="Ctx">The Context type.</typeparam>
        struct Interaction<Ctx>
        {
            public Ctx Context;
            public uint Action;
            public float Probability;
            public string UniqueKey;
        }

        public static void Run()
        {
            string exploration_type = "greedy";

            if (exploration_type == "greedy")
            {
                // Initialize Epsilon-Greedy explore algorithm using built-in StringRecorder and SimpleContext types
                
                // Creates a recorder of built-in StringRecorder type for string serialization
                StringRecorder<SimpleContext> recorder = new StringRecorder<SimpleContext>();
                
                int numActions = 10;
                float epsilon = 0.2f;
		        // Creates an Epsilon-Greedy explorer using the specified settings
                var explorer = new EpsilonGreedyExplorer(epsilon, numActions);

                // Creates an MwtExplorer instance using the recorder above
                // Creates a policy that interacts with SimpleContext type
                var mwtt = MwtExplorer.Create("mwt", recorder, explorer, new StringPolicy());

                // Creates a context of built-in SimpleContext type
                SimpleContext context = new SimpleContext(new float[] { .5f, 1.3f, -.5f });

                // Performs exploration by passing an instance of the Epsilon-Greedy exploration algorithm into MwtExplorer
                // using a sample string to uniquely identify this event
                string uniqueKey = "eventid";
                int action = mwtt.ChooseAction(new UniqueEventID { Key = uniqueKey, TimeStamp = DateTime.UtcNow }, context);

                Console.WriteLine(recorder.GetRecording());

                return;
            }
            else if (exploration_type == "tau-first")
            {
                // Initialize Tau-First explore algorithm using custom Recorder, Policy & Context types
                MyRecorder recorder = new MyRecorder();

                int numActions = 10;
                int tau = 0;
                
                //MwtExplorer<MyContext> mwtt = new MwtExplorer<MyContext>("mwt", recorder);
                var mwtt = MwtExplorer.Create("mwt", recorder, new TauFirstExplorer(tau, numActions), new MyPolicy());

                int action = mwtt.ChooseAction(new UniqueEventID { Key = "key", TimeStamp = DateTime.UtcNow }, new MyContext());
                Console.WriteLine(String.Join(",", recorder.GetAllInteractions().Select(it => it.Action)));
                return;
            }
            else if (exploration_type == "bootstrap")
            {
                // TODO: add support for bootstrap
                //// Initialize Bootstrap explore algorithm using custom Recorder, Policy & Context types
                //MyRecorder recorder = new MyRecorder();
                ////MwtExplorer<MyContext> mwtt = new MwtExplorer<MyContext>("mwt", recorder);

                //uint numActions = 10;
                //uint numbags = 2;
                //MyPolicy[] policies = new MyPolicy[numbags];
                //for (int i = 0; i < numbags; i++)
                //{
                //    policies[i] = new MyPolicy(i * 2);
                //}
                //var mwtt = MwtExplorer.Create("mwt", recorder, new BootstrapExplorer(numActions));
                //uint action = mwtt.ChooseAction(new BootstrapExplorer<MyContext>(policies, numActions), "key", new MyContext());
                //Console.WriteLine(String.Join(",", recorder.GetAllInteractions().Select(it => it.Action)));
                return;
            }
            else if (exploration_type == "softmax")
            {
                // TODO: add support for softmax
                //// Initialize Softmax explore algorithm using custom Recorder, Scorer & Context types
                //MyRecorder recorder = new MyRecorder();
                //MwtExplorer<MyContext> mwtt = new MwtExplorer<MyContext>("mwt", recorder);

                //uint numActions = 10;
                //float lambda = 0.5f;
                //MyScorer scorer = new MyScorer(numActions);
                //uint action = mwtt.ChooseAction(new SoftmaxExplorer<MyContext>(scorer, lambda, numActions), "key", new MyContext());

                //Console.WriteLine(String.Join(",", recorder.GetAllInteractions().Select(it => it.Action)));
                return;
            }
            else if (exploration_type == "generic")
            {
                // TODO: add support for generic
                //// Initialize Generic explore algorithm using custom Recorder, Scorer & Context types
                //MyRecorder recorder = new MyRecorder();
                //MwtExplorer<MyContext> mwtt = new MwtExplorer<MyContext>("mwt", recorder);

                //uint numActions = 10;
                //MyScorer scorer = new MyScorer(numActions);
                //uint action = mwtt.ChooseAction(new GenericExplorer<MyContext>(scorer, numActions), "key", new MyContext());

                //Console.WriteLine(String.Join(",", recorder.GetAllInteractions().Select(it => it.Action)));
                return;
            }
            else
            {  //add error here


            }
        }
    }
}
