using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public class UnauthorizedException : InternHubException
{
    public UnauthorizedException(string message = "You are not authorized to perform this action.")
        : base(message) { }
}
