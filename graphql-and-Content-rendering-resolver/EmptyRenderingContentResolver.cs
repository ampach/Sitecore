namespace {your-namespace}
{
    using Sitecore.LayoutService.Configuration;
    using Sitecore.LayoutService.ItemRendering.ContentsResolvers;
    using Sitecore.Mvc.Presentation;
    using System.Collections.Specialized;

    public class EmptyRenderingContentResolver : IRenderingContentsResolver
    {
        public bool IncludeServerUrlInMediaUrls { get; set; } = true;

        public bool UseContextItem { get; set; }

        public string ItemSelectorQuery { get; set; }

        public NameValueCollection Parameters { get; set; } = new NameValueCollection(0);

        public EmptyRenderingContentResolver()
        {
        }

        public object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            return null;
        }
    }
}