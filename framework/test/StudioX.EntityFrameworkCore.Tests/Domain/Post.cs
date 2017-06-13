﻿using System;
using System.ComponentModel.DataAnnotations;
using StudioX.Domain.Entities.Auditing;

namespace StudioX.EntityFrameworkCore.Tests.Domain
{
    public class Post : AuditedEntity<Guid>
    {
        [Required]
        public Blog Blog { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public Post()
        {
            
        }

        public Post(Blog blog, string title, string body)
        {
            Blog = blog;
            Title = title;
            Body = body;
        }
    }
}