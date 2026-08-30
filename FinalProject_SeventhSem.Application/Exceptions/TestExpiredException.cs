using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public class TestExpiredException : InternHubException
{
    public TestExpiredException()
        : base("The test session has expired. Your answered questions have been automatically submitted.") { }

    public TestExpiredException(string message) : base(message) { }
}

