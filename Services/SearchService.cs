using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using voicio.Models;

namespace voicio.Services
{
    public class SearchService
    {
        public ObservableCollection<string> LastSearches;
        public List<Hint> Search(string Query, bool isFuzzy, bool IsTextSearch, bool IsCommentSearch)
        {
            List<Hint> hints = new List<Hint>();
            using (var DataSource = new HelpContext())
            {
                if (Query.Trim() != "") LastSearches.Insert(0, Query);
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
        public SearchService()
        {
            LastSearches = new ObservableCollection<string>();
        }
    }
}
