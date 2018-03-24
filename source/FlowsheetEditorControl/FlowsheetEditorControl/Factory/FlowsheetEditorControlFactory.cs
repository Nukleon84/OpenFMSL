using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Contracts.Entities;
using FlowsheetEditorControl.ViewModels;
using Caliburn.Micro;

namespace FlowsheetEditorControl.Factory
{
    public class FlowsheetEditorControlFactory : IFlowsheetEntityEditorFactory
    {
        IEventAggregator _aggregator;

        public FlowsheetEditorControlFactory(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        public IFlowsheetEntityEditorViewModel Create(FlowsheetEntity model)
        {
            return new FlowsheetEditorViewModel(_aggregator, model);
        }
    }
}
