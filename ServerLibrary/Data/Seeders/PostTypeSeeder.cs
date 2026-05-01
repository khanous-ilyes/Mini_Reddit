using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLibrary.Entities.Posts;
using ServerLibrary.Data;

namespace ServerLibrary.Data.Seeders;

public static class PostTypeSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!context.PostTypes.Any())
        {
            var seed = new List<PostType>
            {
                new PostType { Id = Guid.NewGuid(), Code = "SOLUTION", Label = "Partage de Solution", AllowComments = true, RequiresPrivilege = false, IsActive = true },
                new PostType { Id = Guid.NewGuid(), Code = "PROBLEM", Label = "Poser un Problème", AllowComments = true, RequiresPrivilege = false, IsActive = true },
                new PostType { Id = Guid.NewGuid(), Code = "NEWS", Label = "Actualité / Info", AllowComments = true, RequiresPrivilege = true, IsActive = true },
                new PostType { Id = Guid.NewGuid(), Code = "QUIZ", Label = "Quiz", AllowComments = false, RequiresPrivilege = true, IsActive = true },
                new PostType { Id = Guid.NewGuid(), Code = "SURVEY", Label = "Sondage", AllowComments = false, RequiresPrivilege = true, IsActive = true }
            };
            
            await context.PostTypes.AddRangeAsync(seed);
            await context.SaveChangesAsync();
        }
    }
}
