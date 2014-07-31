using MediaBrowser.Controller.Net;
using MediaBrowser.Plugins.Spotify.Dto;
using ServiceStack;
using System;

namespace MediaBrowser.Plugins.Spotify.API
{
    [Route("/Spotify/Album/{id}", "GET")]
    [Api(Description = "Gets album information for a given id")]
    public class GetAlbum : IReturn<AlbumDto>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public Guid Id { get; set; }
    }

    public class AlbumService : IRestfulService
    {
        public object Get(GetAlbum request)
        {
            //TODO
            var result = "TODO";
            return result;
        }

    }
}
