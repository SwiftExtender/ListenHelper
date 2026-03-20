using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using voicio.Models;

namespace voicio.Services
{
    public class SearchService
    {
        public ObservableCollection<string> LastVoiceSearches;
        public ObservableCollection<string> LastHintSearches;
        public List<Hint> SearchHint(string Query, bool isFuzzy, bool IsTextSearch = true, bool IsCommentSearch = false)
        {
            List<Hint> hints = new List<Hint>();
            using (var DataSource = new HelpContext())
            {
                if (Query.Trim() != "") LastHintSearches.Insert(0, Query);
                //TagsForChoice = DataSource.TagTable.ToList();
                if (isFuzzy)
                {
                    if (IsTextSearch) hints.AddRange(DataSource.HintTable.Where(b => b.HintText.Contains(Query)).ToList());
                    if (IsCommentSearch) hints.AddRange(DataSource.HintTable.Where(b => b.Comment.Contains(Query)).ToList());
                    //if (IsTagSearch) hints.Add(DataSource.HintTable.Where(b => b.HintTag.Any(pz => pz.Tag.TagText.Contains(Query))).ToList());
                }
                else
                {
                    if (IsTextSearch) hints.AddRange(DataSource.HintTable.Where(b => b.HintText == Query).ToList());
                    if (IsCommentSearch) hints.AddRange(DataSource.HintTable.Where(b => b.Comment == Query).ToList());
                    //if (IsTagSearch) hints.Add(DataSource.HintTable.Where(b => b.HintTag.Any(pz => pz.Tag.TagText == Query)).ToList());
                }
            }
            return hints;
        }
        public List<ScriptCodeModel> SearchScript(string Query, bool isFuzzy)
        {
            List<ScriptCodeModel> scripts = new List<ScriptCodeModel>();
            using (var DataSource = new HelpContext())
            {
                if (Query.Trim() != "") LastVoiceSearches.Insert(0, Query);
                //TagsForChoice = DataSource.TagTable.ToList();
                if (isFuzzy)
                {
                    scripts.AddRange(DataSource.ScriptTable.Where(b => b.Name.Contains(Query)).ToList());
                }
                else
                {
                    scripts.AddRange((DataSource.ScriptTable.Where(b => b.Name == Query)).ToList());
                }
            }
            return scripts;
        }
        public SearchService()
        {
            LastHintSearches = new ObservableCollection<string>();
        }
    }
}
