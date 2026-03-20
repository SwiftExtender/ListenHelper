using Microsoft.EntityFrameworkCore;
using System;

namespace voicio.Models
{
    public class HelpContext : DbContext
    {
        public DbSet<Tag>? TagTable { get; set; }
        public DbSet<Hint>? HintTable { get; set; }
        public DbSet<HintTag>? HintTagTable { get; set; }
        public DbSet<ScriptCodeModel>? ScriptTable { get; set; }
        private string DbPath { get; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
        public HelpContext()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            DbPath = System.IO.Path.Join(path, "helper.db");
        }
    }
}