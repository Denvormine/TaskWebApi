using Microsoft.EntityFrameworkCore;
using TaskRestApi.Data;
using TaskStatus = TaskRestApi.Data.TaskStatus;

namespace TaskRestApi.DbContexts;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<SomeTask>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (TaskStatus)Enum.Parse(typeof(TaskStatus), v));
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<SomeTask> Tasks { get; set; }
}