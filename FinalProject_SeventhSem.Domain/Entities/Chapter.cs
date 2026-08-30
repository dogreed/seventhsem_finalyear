using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Chapter : BaseEntity
{
    public int StackId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Stack Stack { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
