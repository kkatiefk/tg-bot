using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using static tg.MovieSeekerBot;

namespace tg
{


    public class MovieInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("titleNameText")]
        public string TitleNameText { get; set; }

        [JsonProperty("titleReleaseText")]
        public string TitleReleaseText { get; set; }

        [JsonProperty("titleTypeText")]
        public string TitleTypeText { get; set; }

        [JsonProperty("titlePosterImageModel")]
        public TitlePosterImageModel TitlePosterImageModel { get; set; }

        [JsonProperty("topCredits")]
        public List<string> TopCredits { get; set; }

        [JsonProperty("imageType")]
        public string ImageType { get; set; }
    }

    public class TitlePosterImageModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class MovieSearchResult
    {
        [JsonProperty("titleResults")]
        public TitleResults TitleResults { get; set; }
    }

    public class TitleResults
    {
        [JsonProperty("results")]
        public List<MovieInfo> Results { get; set; }
    }


    public class ActorInfo
    {
        [JsonProperty("displayNameText")]
        public string DisplayNameText { get; set; }
        [JsonProperty("knownForJobCategory")]
        public string KnownForJobCategory { get; set; }
        [JsonProperty("knownForTitleText")]
        public string KnownForTitleText { get; set; }
        [JsonProperty("knownForTitleYear")]
        public string KnownForTitleYear { get; set; }
        [JsonProperty("avatarImageModel")]
        public AvatarImageModel AvatarImageModel { get; set; }
    }

    public class AvatarImageModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("maxHeight")]
        public int MaxHeight { get; set; }
        [JsonProperty("maxWidth")]
        public int MaxWidth { get; set; }
        [JsonProperty("caption")]
        public string Caption { get; set; }
    }

    public class ActorSearchResult
    {
        [JsonProperty("nameResults")]
        public NameResults NameResults { get; set; }
    }

    public class NameResults
    {
        [JsonProperty("results")]
        public List<ActorInfo> Results { get; set; }
    }

    public class MovieRatingResponse
    {
        [JsonProperty("filmId")]
        public string FilmId { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }








}
