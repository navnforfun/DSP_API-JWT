using System;
using System.Collections.Generic;

namespace DSP_API.Models.Entity;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool? BanEnabled { get; set; }

    public string? Description { get; set; }

    public string? Img { get; set; }

    public string? JobTitle { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<BoxShare> BoxShares { get; set; } = new List<BoxShare>();

    public virtual ICollection<Box> Boxes { get; set; } = new List<Box>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
