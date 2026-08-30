using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public class InvalidRefreshTokenException : InternHubException
{
    public InvalidRefreshTokenException()
        : base("The refresh token is invalid or has expired. Please log in again.") { }

    public InvalidRefreshTokenException(string message) : base(message) { }
}
