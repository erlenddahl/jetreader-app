using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.DependencyService;

namespace JetReader.UWP.DependencyService {
    public class BatteryProvider : IBatteryProvider {
        public int RemainingChargePercent => 100;
    }
}
