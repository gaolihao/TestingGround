using Microsoft.EntityFrameworkCore;

namespace TodoApi;
public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todo { get; set; }
}