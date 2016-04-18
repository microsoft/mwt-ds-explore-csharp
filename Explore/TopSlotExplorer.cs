﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Research.MultiWorldTesting.ExploreLibrary
{
    // TODO: what about removing this class entirely and refactor the Top Slot logic as static methods instead?
    public sealed class TopSlotExplorer : BaseExplorer<int[], int[]>
    {
        private readonly IVariableActionExplorer<int, int> singleExplorer;

        public TopSlotExplorer(IVariableActionExplorer<int, int> variableActionExplorer, int numActions = int.MaxValue)
            : base(numActions)
        {
            if (variableActionExplorer == null)
                throw new ArgumentNullException("variableActionExplorer");

            this.singleExplorer = variableActionExplorer;
        }

        public override ExplorerDecision<int[]> MapContext(PRG prg, int[] ranking)
        {
            if (ranking == null || ranking.Length < 1)
                throw new ArgumentException("Actions chosen by default policy must not be empty.");

            var decision = this.singleExplorer.Explore(prg, ranking[0], ranking.Length);
            MultiActionHelper.PutActionToList(decision.Value, ranking);

            return ExplorerDecision.Create(ranking, decision.ExplorerState, decision.ShouldRecord);
        }
    }
}