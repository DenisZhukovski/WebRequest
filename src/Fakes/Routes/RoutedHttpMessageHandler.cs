using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebRequest.Elegant.Fakes
{
    public class RoutedHttpMessageHandler : DelegatingHandler
    {
        private readonly IRoute _requestRoute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="responses">The collection of responses for http requests.</param>
        public RoutedHttpMessageHandler(params IResponse[] responses)
            : this(new Route(responses))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="requestRoute">Manages the responses for http requests.</param>
        public RoutedHttpMessageHandler(IRoute requestRoute)
            : this(requestRoute, new HttpClientHandler())
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="requestRoute">Manages the responses for http requests.</param>
        /// <param name="inner">Inner http message handler</param>
        public RoutedHttpMessageHandler(IRoute requestRoute, HttpMessageHandler inner)
            : base(inner)
        {
            _requestRoute = requestRoute;
        }

        /// <summary>
        /// Simulates sending requests to the server responding with prepared http messages base on configured routes.
        /// </summary>
        /// <param name="request">The request that has to be sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Simulated http response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await _requestRoute.MatchesAsync(request).ConfigureAwait(false)
                ? await _requestRoute.ResponseAsync(request).ConfigureAwait(false)
                : await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
