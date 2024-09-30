using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Emby.Web.GenericEdit;
using Emby.Web.GenericEdit.Elements;
using Emby.Web.GenericEdit.Elements.List;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Attributes;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace EmbyPluginUiDemo.UI.Lists
{
    public class ListsUI : EditableOptionsBase
    {
        public enum DemoChoices
        {
            [Description("Movie(s) List")]
            MovieList,
        }

        public ListsUI()
        {
            this.ActivityList = new GenericItemList();
        }

        public override string EditorTitle => "List Display";

        public override string EditorDescription => string.Empty;

        public SpacerItem Spacer1 { get; set; } = new SpacerItem();

        [DisplayName("Choose a Demo to Show")]
        [AutoPostBack(ListsPageView.PostBackCommand, nameof(DemoChoice))]
        public DemoChoices DemoChoice { get; set; } = DemoChoices.MovieList;

        [VisibleCondition(nameof(DemoChoice), ValueCondition.IsEqual, nameof(DemoChoices.MovieList))]
        public CaptionItem CaptionStrings { get; set; } = new CaptionItem("Movie List");

        [DisplayName("Movies")]
        [Description("Shows the list of movies in your library")]
        [VisibleCondition(nameof(DemoChoice), ValueCondition.IsEqual, nameof(DemoChoices.MovieList))]
        public GenericItemList ActivityList { get; set; }

        public void CreateListItems(IActivityManager activityManager, ILibraryManager libraryManager)
        {
            this.ActivityList.Clear();

            if (this.DemoChoice == DemoChoices.MovieList)
            {
                var query = new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { "Movie" },
                    Recursive = true,
                };

                var movies = libraryManager.GetItemList(query);

                foreach (var movie in movies)
                {
                    // Cast the movie to Video to access media streams
                    var video = movie as MediaBrowser.Controller.Entities.Video;
                    if (video != null)
                    {
                        // Get subtitle streams
                        var mediaStreams = video.GetMediaStreams();
                        var subtitleStreams = mediaStreams.Where(s => s.Type == MediaBrowser.Model.Entities.MediaStreamType.Subtitle);

                        // Add the movie to the list with subtitle information
                        this.ActivityList.Add(CreateListItem(video, subtitleStreams));
                    }
                }
            }
        }

        private GenericListItem CreateListItem(Video movie, IEnumerable<MediaStream> subtitleStreams)
        {
            string productionYear = movie.ProductionYear.HasValue ? movie.ProductionYear.Value.ToString() : "Année inconnue";

            string subtitlesInfo;
            if (subtitleStreams.Any())
            {
                var subtitleList = new List<string>();
                int index = 1;
                foreach (var s in subtitleStreams)
                {
                    string displayTitle = s.DisplayTitle ?? "Titre inconnu";
                    string language = s.Language ?? "Langue inconnue";
                    subtitleList.Add($"Subtitle {index} : {displayTitle}, {language}");
                    index++;
                }

                subtitlesInfo = "Sous-titres :\n" + string.Join("\n", subtitleList);

            }
            else
            {
                subtitlesInfo = "Aucun sous-titre disponible";
            }

            // Combiner l'année de production et les informations sur les sous-titres
            string secondaryText = $"{productionYear} | {subtitlesInfo}";

            return new GenericListItem
            {
                PrimaryText = movie.Name,
                SecondaryText = secondaryText,
                Icon = IconNames.image,
                IconMode = ItemListIconMode.LargeRegular,
                HyperLink = $"/emby/EmbyPluginUiDemo/Subtitles?id={movie.Id}",
            };
        }



    }
}
