using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Skills;

public record CreateSkillRequest(
    string Name
);

public record UpdateSkillRequest(
    string Name
);

public record CreateSkillAliasRequest(
    int SkillId,
    string Alias
);


public record SkillResponse(
    int SkillId,
    string Name,
    IReadOnlyList<string> Aliases
);

public record SkillAliasResponse(
    int AliasId,
    int SkillId,
    string SkillName,
    string Alias
);
