using Microsoft.Research.MultiWorldTesting.ExploreLibrary;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary.SingleAction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCommon
{
    public interface IVariableActionContext
    {
        uint GetNumberOfActions();
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
        public TestScorer(int param, uint numActions, bool uniform = true)
        {
            this.param = param;
            this.uniform = uniform;
            this.numActions = numActions;
        }
        public List<float> ScoreActions(Ctx context, uint numActionsVariable = uint.MaxValue)
        {
            if (uniform)
            {
                return Enumerable.Repeat<float>(param, (int)numActions).ToList();
            }
            else
            {
                return Array.ConvertAll<int, float>(Enumerable.Range(param, (int)numActions).ToArray(), Convert.ToSingle).ToList();
            }
        }
        private int param;
        private uint numActions;
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
        public VariableActionTestContext(uint numberOfActions)
        {
            NumberOfActions = numberOfActions;
        }

        public uint GetNumberOfActions()
        {
            return NumberOfActions;
        }

        public uint NumberOfActions { get; set; }
    }
}
