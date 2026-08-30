using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Exceptions;

public class OrganizationNotVerifiedException : InternHubException
{
    public OrganizationNotVerifiedException()
        : base("Your organization account is pending verification by an administrator.") { }
}