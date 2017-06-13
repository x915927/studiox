﻿using StudioX.Events.Bus;

namespace StudioX.EntityFrameworkCore.Dapper.Tests.Domain
{
    public class BlogUrlChangedEventData : EventData
    {
        public Blog Blog { get; private set; }

        public string OldUrl { get; private set; }

        public BlogUrlChangedEventData(Blog blog, string oldUrl)
        {
            Blog = blog;
            OldUrl = oldUrl;
        }
    }
}