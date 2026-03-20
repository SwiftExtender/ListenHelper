using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace voicio.Models
{
    public class ScriptCodeModel
    {
        [Key, Required]
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsArgsRequired { get; set; } = false;
        public string? Name { get; set; }
        public string? SourceCode { get; set; } = "";
        public string? Checksum { get; set; }
        public string? HotKey { get; set; }
        public byte[]? BinaryExecutable { get; set; }
        [NotMapped]
        public bool IsSaved { get; set; } = true; //false = temp, true = in DB
        public ScriptCodeModel(bool isSaved)
        {
            IsSaved = isSaved;
        }
        public ScriptCodeModel(bool isSaved, string code)
        {
            IsSaved = isSaved;
            SourceCode = code;
        }
        public ScriptCodeModel(int id, bool isActive, string name)
        {
            Id = id;
            IsActive = isActive;
            Name = name;
        }
    }
}

