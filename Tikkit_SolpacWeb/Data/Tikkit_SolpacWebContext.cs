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

        public DbSet<Tikkit_SolpacWeb.Models.Requests> Requests { get; set; } = default!;

        public DbSet<Tikkit_SolpacWeb.Models.Users>? Users { get; set; }
        public DbSet<Tikkit_SolpacWeb.Models.Notification> Notification { get; set; }
    }
}
