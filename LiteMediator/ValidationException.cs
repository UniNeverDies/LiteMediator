using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteMediator
{
    public class ValidationException(IEnumerable<string> errors) : Exception("Validation failed.")
    {
        public IReadOnlyList<string> Errors { get; } = errors?.ToList() ?? [];
    }
}
