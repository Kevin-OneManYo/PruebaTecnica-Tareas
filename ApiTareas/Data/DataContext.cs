using ApiTareas.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ApiTareas.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Todo> TodoList => Set<Todo>();
    }
}
