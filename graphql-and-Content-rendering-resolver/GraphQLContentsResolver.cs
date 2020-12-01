namespace {your-namespace}
{
    using Sitecore.JavaScriptServices.GraphQL.LayoutService;
    using Sitecore.LayoutService.Presentation.Pipelines.RenderJsonRendering;
    using Newtonsoft.Json.Linq;

    public class GraphQLContentsResolver : ResolveRenderingContents
    {
        private readonly GraphQLAwareRenderingContentsResolver _graphQLAwareRenderingContentsResolver;

        public GraphQLContentsResolver(Sitecore.LayoutService.Configuration.IConfiguration configuration,
          Sitecore.JavaScriptServices.Configuration.IConfigurationResolver configurationResolver,
          Sitecore.Services.GraphQL.Hosting.Configuration.IGraphQLEndpointManager graphQLEndpointManager,
          Sitecore.Services.GraphQL.Hosting.IDocumentWriter documentWriter,
          Sitecore.Abstractions.BaseLog log,
          Sitecore.JavaScriptServices.GraphQL.Helpers.IAsyncHelpers asyncHelpers) : base(configuration)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(configurationResolver, nameof(configurationResolver));
            Sitecore.Diagnostics.Assert.ArgumentNotNull(graphQLEndpointManager, nameof(graphQLEndpointManager));
            Sitecore.Diagnostics.Assert.ArgumentNotNull(documentWriter, nameof(documentWriter));
            Sitecore.Diagnostics.Assert.ArgumentNotNull(log, nameof(log));
            Sitecore.Diagnostics.Assert.ArgumentNotNull(asyncHelpers, nameof(asyncHelpers));

            _graphQLAwareRenderingContentsResolver = new GraphQLAwareRenderingContentsResolver(configurationResolver, graphQLEndpointManager, 
                documentWriter, log, asyncHelpers);
        }

        protected override void SetResult(RenderJsonRenderingArgs args)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(args, nameof(args));
            Sitecore.Diagnostics.Assert.IsNotNull(args.Result, "args.Result cannot be null");
            args.Result.Contents = GetResolvedContents(args);
        }

        protected override object GetResolvedContents(RenderJsonRenderingArgs args)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(args, nameof(args));

            var grqlResult = _graphQLAwareRenderingContentsResolver.ResolveContents(args.Rendering, args.RenderingConfiguration) as JObject;

            if (grqlResult == null)
            {
                return args.Result.Contents;
            }

            var mergedResult = new JObject();
            mergedResult.Merge((JObject)args.Result.Contents);
            mergedResult.Merge(grqlResult["data"]);
            return mergedResult;
        }
    }
}