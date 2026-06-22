using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public readonly record struct Item(string Type, string Name, int Quantity, int Value, string Description);
}
