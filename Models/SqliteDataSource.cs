using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace voicio.Models
{
    public class Tag
    {
        [Key, Required]
        public int Id { get; set; }
        public string? TagText { get; set; }
        public List<HintTag> HintTag { get; } = new();
        [NotMapped]
        public bool IsSaved { get; set; } = true; //false = temp, true = in DB
        public Tag(bool isSaved)
        {
            IsSaved = isSaved;
        }
        public Tag()
        {
            IsSaved = true;
        }
    }
    public class Hint
    {
        [Key, Required]
        public int Id { get; set; }
        public string? HintText { 
            get; 
            set; 
        }
        public string? Comment { 
            get; 
            set; 
        }
        public List<HintTag> HintTag { get; } = new();
        [NotMapped]
        public bool IsSaved { get; set; } = true; //false = temp, true = in DB
        public Hint(bool isSaved)
        {
            IsSaved = isSaved;
        }
        public Hint(string hintText, string comment)
        {
            HintText = hintText;
            Comment = comment;
        }
        public Hint(int id, string hintText, string comment)
        {
            Id = id;
            HintText = hintText;
            Comment = comment;
        }
    }
    public class HintTag
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public int HintId { get; set; }
        public Tag Tag { get; set; } = null!;
        public Hint Hint { get; set; } = null!;
        public HintTag() { }
        public HintTag(int id) {
            Id = id;
        }
    }   
    public class VoiceSetting
    {
        [Key, Required]
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? Command { get; set; }
        public string? SourceCode { get; set; } = "";
        public byte[]? ActionTreeExpression { get; set; }
        [NotMapped]
        public bool IsSaved { get; set; } = true; //false = temp, true = in DB
        public VoiceSetting(bool isSaved)
        {
            IsSaved = isSaved;
        }
        public VoiceSetting(int id, bool isActive, string description, string command)
        {
            Id = id;
            IsActive = isActive;
            Description = description;
            Command = command;
        }
    }
    public class HelpContext : DbContext
    {
        public DbSet<Tag>? TagTable { get; set; }
        public DbSet<Hint>? HintTable { get; set; }
        public DbSet<HintTag>? HintTagTable { get; set; }
        public DbSet<VoiceSetting>? VoiceSettingTable { get; set; }
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