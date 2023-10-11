﻿using System;
using System.Collections.Generic;

namespace DSP_API.Models.Entity;

public partial class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public int BoxId { get; set; }

    public int UserId { get; set; }

    public virtual Box Box { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
