using Microsoft.Research.MultiWorldTesting.ExploreLibrary;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary.SingleAction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCommon
{
    public interface IVariableActionContext
    {
        int GetNumberOfActions();
    }

    public class VariableActionProvider<TContext> : INumberOfActionsProvider<TContext>
    {
        public int GetNumberOfActions(TContext context)
        {
            return ((IVariableActionContext)context).GetNumberOfActions();
        }
    }

    public class TestPolicy<TContext> : IPolicy<TContext>
    {
        public TestPolicy() : this(-1) { }

        public TestPolicy(int index)
        {
            this.index = index;
            this.ActionToChoose = uint.MaxValue;
        }

        public PolicyDecision<int> MapContext(TContext context)
        {
            return (this.ActionToChoose != uint.MaxValue) ? (int)this.ActionToChoose : 5;
        }

        public uint ActionToChoose { get; set; }
        private int index;
    }

    public class TestSimplePolicy : IPolicy<SimpleContext>
    {
        public PolicyDecision<int> MapContext(SimpleContext context)
        {
            return 1;
        }
    }

    public class StringPolicy : IPolicy<SimpleContext>
    {
        public PolicyDecision<int> MapContext(SimpleContext context)
        {
            return 1;
        }
    }

    public class TestScorer<Ctx> : IScorer<Ctx>
    {
        public TestScorer(int param, int numActions, bool uniform = true)
        {
            this.param = param;
            this.uniform = uniform;
            this.numActions = numActions;
        }

        public PolicyDecision<float[]> MapContext(Ctx context)
        {
            if (uniform)
            {
                return Enumerable.Repeat<float>(param, numActions).ToArray();
            }
            else
            {
                return Array.ConvertAll<int, float>(Enumerable.Range(param, numActions).ToArray(), Convert.ToSingle).ToArray();
            }
        }

        private int param;
        private int numActions;
        private bool uniform;
    }

    public class RegularTestContext
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public override string ToString()
        {
            return id.ToString();
        }
    }

    public class VariableActionTestContext : RegularTestContext, IVariableActionContext
    {
        public VariableActionTestContext(int numberOfActions)
        {
            NumberOfActions = numberOfActions;
        }

        public int GetNumberOfActions()
        {
            return NumberOfActions;
        }

        public int NumberOfActions { get; set; }
    }
}
