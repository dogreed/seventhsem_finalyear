using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public abstract class InternHubException : Exception
{
    protected InternHubException(string message) : base(message) { }
    protected InternHubException(string message, Exception inner) : base(message, inner) { }
}
