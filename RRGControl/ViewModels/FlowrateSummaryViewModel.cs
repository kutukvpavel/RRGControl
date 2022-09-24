using System.Collections.Generic;
using System.Linq;

namespace RRGControl.ViewModels
{
    public class FlowrateSummaryViewModel : ViewModelBase
    {
        public FlowrateSummaryViewModel(IEnumerable<ConnectionViewModel> u)
        {
            foreach (var item in u)
            {
                Units.AddRange(item.Units.Select(x => new UnitFlowrateViewModel(x)));
            }
        }

        public List<UnitFlowrateViewModel> Units { get; } = new List<UnitFlowrateViewModel>();
    }
}
