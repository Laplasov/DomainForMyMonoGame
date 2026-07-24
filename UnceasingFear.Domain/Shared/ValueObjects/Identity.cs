using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public enum UnitType { Major, Minor, Apex, None }
    public enum Element { Sanguis, Vigor, Vis, Tutamen, Anima, Celeritas, Nihil, None }
    public readonly record struct Identity
    {
        public Element Element { get; init; }      
        public UnitType Type { get; init; }        
        public int Tier { get; init; }

        public bool Matches(Identity requested)
        {
            if (this.Element != Element.None && this.Element != requested.Element) return false;
            if (this.Type != UnitType.None && this.Type != requested.Type) return false;
            if (this.Tier != 0 && this.Tier != requested.Tier) return false;

            return true;
        }
    }
}
