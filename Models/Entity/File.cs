using System;
using System.Collections.Generic;

namespace DSP_API.Models.Entity;

public partial class File
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Size { get; set; }

    public DateTime DatePost { get; set; }

    public int BoxId { get; set; }

    public int View { get; set; }

    public virtual Box Box { get; set; } = null!;
}
