using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public class RateLimitException : InternHubException
{
    public RateLimitException(string message = "Too many requests. Please try again later.")
        : base(message) { }
}
