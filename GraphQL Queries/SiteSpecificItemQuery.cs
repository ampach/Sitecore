using System;
using GraphQL.Types;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Mvc.Extensions;
using Sitecore.Services.GraphQL.Content;
using Sitecore.Services.GraphQL.Content.GraphTypes;
using Sitecore.Services.GraphQL.Schemas;
using Sitecore.Sites;

public class SiteSpecificItemQuery : RootFieldType<ItemInterfaceGraphType, Item>, IContentSchemaRootFieldType
{
	public SiteSpecificItemQuery()
		: base("siteSpecificItem", "Allows querying items from the content tree beneath specific site")
	{
		var queryArgumentArray = new QueryArgument[5];
		var pathArgument =
			new QueryArgument<StringGraphType> { Name = "path", Description = "The item path or ID to get" };
		queryArgumentArray[0] = pathArgument;
		var languageArgument = new QueryArgument<StringGraphType>
		{
			Name = "language",
			Description = "The item language to request (defaults to the default language)"
		};
		queryArgumentArray[1] = languageArgument;
		var versionArgument = new QueryArgument<IntGraphType>
		{
			Name = "version",
			Description = "The item version to request (if not set, latest version is returned)"
		};
		queryArgumentArray[2] = versionArgument;
		var siteArgument = new QueryArgument<StringGraphType>
		{
			Name = "site",
			Description = "The site name to request (if not set, will be used context one)"
		};
		queryArgumentArray[3] = siteArgument;
		var queryArgument = new QueryArgument<StringGraphType>
		{
			Name = "query",
			Description = "The Sitecore query"
		};
		queryArgumentArray[4] = queryArgument;
		this.Arguments = new QueryArguments(queryArgumentArray);
	}

	protected override Item Resolve(ResolveFieldContext context)
	{
		//get item from Sitecore query
		var query = context.GetArgument<string>("query", (string)null);
		if (!string.IsNullOrWhiteSpace(query))
		{
			var queryItem = Sitecore.Context.Item.Axes.SelectSingleItem(query);
			if (queryItem != null)
				return queryItem;
		}

		var name = context.GetArgument<string>("language", (string)null);
		Language result = null;
		if (name != null && !Language.TryParse(name, out result))
			throw new InvalidOperationException("Unable to parse requested language.");
		if (result == null)
		{
			Language language = Context.Language;
			if ((object)language == null)
				language = LanguageManager.DefaultLanguage;
			result = language;
		}
		var num = context.GetArgument<int?>("version", new int?()) ?? -1;
		var siteName = context.GetArgument<string>("site", (string)null);
		string siteRootPath;
		if (string.IsNullOrEmpty(siteName))
		{
			siteRootPath = Context.Site?.RootPath;
		}
		else
		{
			var site = SiteManager.GetSite(siteName);
			siteRootPath = site?.Properties["rootPath"]?.ValueOrEmpty();
		}

		if (string.IsNullOrEmpty(siteRootPath))
			return null;
		var relativePath = context.GetArgument("path", (string)null);
		var inputPath = $"{siteRootPath}{relativePath}";
		Item obj;
		if (!IdHelper.TryResolveItem(this.Database, inputPath, result, new int?(num), out obj))
			return null;
		if (obj == null || obj.Versions.Count == 0)
			return null;
		return obj;
	}

	public Database Database { get; set; }
}