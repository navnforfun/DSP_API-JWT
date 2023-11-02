using System;
using System.Collections.Generic;

namespace DSP_API.Models.Entity;

public partial class BoxShare
{
    public int BoxId { get; set; }

    public int UserId { get; set; }

    public bool? EditAccess { get; set; }

    public virtual Box Box { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
