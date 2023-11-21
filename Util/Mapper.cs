using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DSP_API.Models.Entity;

namespace DSP_API.Util
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<User, UserDto>();
            CreateMap<Box, BoxDto>();

        }
    }
    public class UserDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;


        public bool? BanEnabled { get; set; }

        public string? Description { get; set; }

        public string? Img { get; set; }

        public string? JobTitle { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        // public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
    public class BoxDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Content { get; set; }

        public string? Url { get; set; }

        public int? UserId { get; set; }

        public DateTime? DateCreated { get; set; }

        public int? View { get; set; }

        public string? Img { get; set; }

        public bool? AdminBan { get; set; }

        public bool? SharedStatus { get; set; }

        // public virtual ICollection<BoxShare> BoxShares { get; set; } = new List<BoxShare>();

        // public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // public virtual ICollection<Models.Entity.File> Files { get; set; } = new List<Models.Entity.File>();
        // public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}