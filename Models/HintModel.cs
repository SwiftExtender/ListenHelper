using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace voicio.Models
{
    public class Hint
    {
        [Key, Required]
        public int Id { get; set; }
        public string? HintText
        {
            get;
            set;
        }
        public string? Comment
        {
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
        public HintTag(int id)
        {
            Id = id;
        }
    }
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
}
