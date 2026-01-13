using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Relation = WebApi.Service.DataAccess.Entity.Relation;

namespace WebApi.Service.DataAccess;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Relation>()
            .Ignore(r => r.Attributes);

        modelBuilder.Entity<Relation>()
            .Property<Dictionary<string, string>>(nameof(Relation.AttributesMap))
            .HasColumnName("attributes")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v)!
            );
    }
}