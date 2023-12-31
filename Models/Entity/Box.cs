﻿using System;
using System.Collections.Generic;

namespace DSP_API.Models.Entity;

public partial class Box
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? ShareCode { get; set; }

    public string? Url { get; set; }

    public int? UserId { get; set; }

    public DateTime? DateCreated { get; set; }

    public int? View { get; set; }

    public string? Img { get; set; }

    public bool? AdminBan { get; set; }

    public bool? SharedStatus { get; set; }

    public string? ShareView { get; set; }

    public string? ShareEdit { get; set; }

    public virtual ICollection<BoxShare> BoxShares { get; set; } = new List<BoxShare>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual User? User { get; set; }

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
