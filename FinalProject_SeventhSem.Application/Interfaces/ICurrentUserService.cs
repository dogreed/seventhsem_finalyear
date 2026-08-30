using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
}