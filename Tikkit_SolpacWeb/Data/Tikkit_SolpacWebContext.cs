using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tikkit_SolpacWeb.Models;

namespace Tikkit_SolpacWeb.Data
{
    public class Tikkit_SolpacWebContext : DbContext
    {
        public Tikkit_SolpacWebContext (DbContextOptions<Tikkit_SolpacWebContext> options)
            : base(options)
        {
        }

        public DbSet<Requests> Requests { get; set; } = default!;

        public DbSet<Users>? Users { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Partners> Partners { get; set; }
        public DbSet<Projects> Projects { get; set; }
    }
}
