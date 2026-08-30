using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public class BadRequestException : InternHubException
{
    public BadRequestException(string message) : base(message) { }
}

